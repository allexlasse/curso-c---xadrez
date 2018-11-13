using System;
using System.Collections.Generic;
using tabuleiro;

namespace xadrez
{
    class PartidaDeXadrez
    {
        public Tabuleiro tab { get; private set; }
        public int turno { get; private set; }
        public Cor jogadorAtual { get; private set; }
        public bool terminada { get; private set; }
        private HashSet<Peca> pecas;
        private HashSet<Peca> capturadas;
        public bool xeque { get; private set; }
        public Peca vulneravelEnPassant { get; private set; }

        public PartidaDeXadrez()
        {
            tab = new Tabuleiro(8, 8);
            turno = 1;
            jogadorAtual = Cor.Branco;
            terminada = false;
            xeque = false;
            vulneravelEnPassant = null;
            pecas = new HashSet<Peca>();
            capturadas = new HashSet<Peca>();
            colocarPecas();
        }

        public Peca executaMovimento(Posicao origem, Posicao destino)
        {
            Peca p = tab.retirarPeca(origem);
            p.incrementarQteMovimentos();
            Peca pecaCapturada = tab.retirarPeca(destino);
            tab.colocarPeca(p, destino);
            if(pecaCapturada != null)
            {
                capturadas.Add(pecaCapturada);
            }

            // #jogadaespecial roque pequeno
            if(p is Rei && destino.coluna == origem.coluna + 2)
            {
                Posicao origemTorre = new Posicao(origem.linha, origem.coluna + 3);
                Posicao destinoTorre = new Posicao(origem.linha, origem.coluna + 1);
                Peca T = tab.retirarPeca(origemTorre);
                T.incrementarQteMovimentos();
                tab.colocarPeca(T, destinoTorre);
            }

            // #jogadaespecial roque grande
            if (p is Rei && destino.coluna == origem.coluna - 2)
            {
                Posicao origemTorre = new Posicao(origem.linha, origem.coluna - 4);
                Posicao destinoTorre = new Posicao(origem.linha, origem.coluna - 1);
                Peca T = tab.retirarPeca(origemTorre);
                T.incrementarQteMovimentos();
                tab.colocarPeca(T, destinoTorre);
            }

            // #jogadaespecial en passant
            if (p is Peao)
            {
                if (origem.coluna != destino.coluna && pecaCapturada == null)
                {
                    Posicao posP;
                    if (p.cor == Cor.Branco)
                    {
                        posP = new Posicao(destino.linha + 1, destino.coluna);
                    }
                    else
                    {
                        posP = new Posicao(destino.linha - 1, destino.coluna);
                    }
                    pecaCapturada = tab.retirarPeca(posP);
                    capturadas.Add(pecaCapturada);
                }
            }

            return pecaCapturada;
        }

        public void desfazMovimento(Posicao origem, Posicao destino, Peca pecaCapturada)
        {
            Peca p = tab.retirarPeca(destino);
            p.decrementarQteMovimentos();
            if(pecaCapturada != null)
            {
                tab.colocarPeca(pecaCapturada, destino);
                capturadas.Remove(pecaCapturada);
            }
            tab.colocarPeca(p, origem);

            // #jogadaespecial roque pequeno
            if (p is Rei && destino.coluna == origem.coluna + 2)
            {
                Posicao origemTorre = new Posicao(origem.linha, origem.coluna + 3);
                Posicao destinoTorre = new Posicao(origem.linha, origem.coluna + 1);
                Peca T = tab.retirarPeca(destinoTorre);
                T.decrementarQteMovimentos();
                tab.colocarPeca(T, origemTorre);
            }

            // #jogadaespecial roque grande
            if (p is Rei && destino.coluna == origem.coluna - 2)
            {
                Posicao origemTorre = new Posicao(origem.linha, origem.coluna - 4);
                Posicao destinoTorre = new Posicao(origem.linha, origem.coluna - 1);
                Peca T = tab.retirarPeca(destinoTorre);
                T.decrementarQteMovimentos();
                tab.colocarPeca(T, origemTorre);
            }

            // #jogadaespecial en passant
            if(p is Peao)
            {
                if(origem.coluna != destino.coluna && pecaCapturada == vulneravelEnPassant)
                {
                    Peca peao = tab.retirarPeca(destino);
                    Posicao posP;
                    if(p.cor == Cor.Branco)
                    {
                        posP = new Posicao(3, destino.coluna);
                    }
                    else
                    {
                        posP = new Posicao(4, destino.coluna);
                    }
                    tab.colocarPeca(peao, posP);
                }
            }
            
        }

        public void realizaJogada(Posicao origem, Posicao destino)
        {
            Peca pecaCapturada = executaMovimento(origem, destino);
            Peca p = tab.peca(destino);

            if (estaEmXeque(jogadorAtual))
            {
                desfazMovimento(origem, destino, pecaCapturada);
                throw new TabuleiroException("Você não pode se colocar em xeque");
            }

            // #jogadaespecial promocao
            if(p is Peao)
            {
                if((p.cor == Cor.Branco && destino.linha == 0) || (p.cor == Cor.Preto && destino.linha == 7))
                {
                    p = tab.retirarPeca(destino);
                    pecas.Remove(p);
                    Peca dama = new Dama(tab, p.cor);
                    tab.colocarPeca(dama, destino);
                    pecas.Add(dama);
                }
            }

            if (estaEmXeque(adversario(jogadorAtual)))
            {
                xeque = true;
            }
            else
            {
                xeque = false;
            }

            if (testeXequemate(adversario(jogadorAtual)))
            {
                terminada = true;
            }
            else
            {
                turno++;
                mudaJogador();
            }

            // #jogadaespecial en passant
            if(p is Peao && (destino.linha == origem.linha - 2 || destino.linha == origem.linha + 2))
            {
                vulneravelEnPassant = p;
            }
            else
            {
                vulneravelEnPassant = null;
            }
            
        }

        private void mudaJogador()
        {
            if(jogadorAtual == Cor.Branco)
            {
                jogadorAtual = Cor.Preto;
            }
            else
            {
                jogadorAtual = Cor.Branco;
            }
        }

        public void validarPosicaoDeOrigem(Posicao pos)
        {
            if(tab.peca(pos) == null)
            {
                throw new TabuleiroException("Não existe peça na posição escolhida");
            }
            if(jogadorAtual != tab.peca(pos).cor)
            {
                throw new TabuleiroException("A peça escolhida não é sua");
            }
            if (!tab.peca(pos).existeMovimentosPossiveis())
            {
                throw new TabuleiroException("Esta peça não pode mover");
            }
        }

        public void validarPosicaoDeDestino(Posicao origem, Posicao destino)
        {
            if (!tab.peca(origem).podeMoverPara(destino))
            {
                throw new TabuleiroException("Posição inválida");
            }
        }

        private Cor adversario (Cor cor)
        {
            if(cor == Cor.Branco)
            {
                return Cor.Preto;
            }
            else
            {
                return Cor.Branco;
            }
        }

        private Peca rei (Cor cor)
        {
            foreach(Peca x in pecasEmJogo(cor))
            {
                if(x is Rei)
                {
                    return x;
                }
            }
            return null;
        }

        public bool estaEmXeque(Cor cor)
        {
            Peca R = rei(cor);
            if(R == null)
            {
                throw new TabuleiroException("Não existe rei da cor " + cor + "em jogo.");
            }

            foreach(Peca x in pecasEmJogo(adversario(cor)))
            {
                bool[,] mat = x.movimentosPossiveis();
                if(mat[R.posicao.linha,R.posicao.coluna])
                {
                    return true;
                }
            }
            return false;
        }

        public bool testeXequemate(Cor cor)
        {
            if (!estaEmXeque(cor))
            {
                return false;
            }

            foreach(Peca x in pecasEmJogo(cor))
            {
                bool[,] mat = x.movimentosPossiveis();
                for(int i=0; i < tab.linhas; i++)
                {
                    for(int j = 0; j < tab.colunas; j++)
                    {
                        if (mat[i, j])
                        {
                            Posicao origem = x.posicao;
                            Posicao destino = new Posicao(i, j);
                            Peca pecaCapturada = executaMovimento(origem, destino);
                            bool testeXeque = estaEmXeque(cor);
                            desfazMovimento(origem, destino, pecaCapturada);
                            if (!testeXeque)
                            {
                                return false;
                            }
                        }
                    }
                }
            }

            return true;
        }

        public HashSet<Peca> pecasCapturadas(Cor cor)
        {
            HashSet<Peca> aux = new HashSet<Peca>();
            foreach(Peca x in capturadas)
            {
                if(x.cor == cor)
                {
                    aux.Add(x);
                }
            }

            return aux;
        }

        public HashSet<Peca> pecasEmJogo(Cor cor)
        {
            HashSet<Peca> aux = new HashSet<Peca>();
            foreach (Peca x in pecas)
            {
                if (x.cor == cor)
                {
                    aux.Add(x);
                }
            }
            aux.ExceptWith(pecasCapturadas(cor));
            return aux;
        }

        public void colocarNovaPeca(Peca peca, char coluna, int linha)
        {
            tab.colocarPeca(peca, new PosicaoXadrez(coluna, linha).toPosicao());
            pecas.Add(peca);
        }

        private void colocarPecas()
        {
            //Brancas
            colocarNovaPeca(new Torre(tab, Cor.Branco), 'a', 1);
            colocarNovaPeca(new Cavaleiro(tab, Cor.Branco), 'b', 1);
            colocarNovaPeca(new Bispo(tab, Cor.Branco), 'c', 1);
            colocarNovaPeca(new Dama(tab, Cor.Branco), 'd', 1);
            colocarNovaPeca(new Rei(tab, Cor.Branco, this), 'e', 1);
            colocarNovaPeca(new Bispo(tab, Cor.Branco), 'f', 1);
            colocarNovaPeca(new Cavaleiro(tab, Cor.Branco), 'g', 1);
            colocarNovaPeca(new Torre(tab, Cor.Branco), 'h', 1);
            colocarNovaPeca(new Peao(tab, Cor.Branco, this), 'a', 2);
            colocarNovaPeca(new Peao(tab, Cor.Branco, this), 'b', 2);
            colocarNovaPeca(new Peao(tab, Cor.Branco, this), 'c', 2);
            colocarNovaPeca(new Peao(tab, Cor.Branco, this), 'd', 2);
            colocarNovaPeca(new Peao(tab, Cor.Branco, this), 'e', 2);
            colocarNovaPeca(new Peao(tab, Cor.Branco, this), 'f', 2);
            colocarNovaPeca(new Peao(tab, Cor.Branco, this), 'g', 2);
            colocarNovaPeca(new Peao(tab, Cor.Branco, this), 'h', 2);

            //Pretas
            colocarNovaPeca(new Torre(tab, Cor.Preto), 'a', 8);
            colocarNovaPeca(new Cavaleiro(tab, Cor.Preto), 'b', 8);
            colocarNovaPeca(new Bispo(tab, Cor.Preto), 'c', 8);
            colocarNovaPeca(new Dama(tab, Cor.Preto), 'd', 8);
            colocarNovaPeca(new Rei(tab, Cor.Preto, this), 'e', 8);
            colocarNovaPeca(new Bispo(tab, Cor.Preto), 'f', 8);
            colocarNovaPeca(new Cavaleiro(tab, Cor.Preto), 'g', 8);
            colocarNovaPeca(new Torre(tab, Cor.Preto), 'h', 8);
            colocarNovaPeca(new Peao(tab, Cor.Preto, this), 'a', 7);
            colocarNovaPeca(new Peao(tab, Cor.Preto, this), 'b', 7);
            colocarNovaPeca(new Peao(tab, Cor.Preto, this), 'c', 7);
            colocarNovaPeca(new Peao(tab, Cor.Preto, this), 'd', 7);
            colocarNovaPeca(new Peao(tab, Cor.Preto, this), 'e', 7);
            colocarNovaPeca(new Peao(tab, Cor.Preto, this), 'f', 7);
            colocarNovaPeca(new Peao(tab, Cor.Preto, this), 'g', 7);
            colocarNovaPeca(new Peao(tab, Cor.Preto, this), 'h', 7);
        }

    }
}
