﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using KadoshModas.DML;
using KadoshModas.BLL;

namespace KadoshModas.UI.UserControls
{
    public partial class ucParcelaVenda : UserControl
    {
        #region Construtor
        public ucParcelaVenda()
        {
            InitializeComponent();
        }
        #endregion

        #region Propriedades
        private DmoParcela _parcela;

        /// <summary>
        /// Propriedade DmoParcela com todas as informações da Parcela
        /// </summary>
        public DmoParcela Parcela
        {
            get { return _parcela; }
            set
            {
                _parcela = value;

                lblNumeroParcela.Text = Parcela.Parcela.ToString();
                lblValorParcela.Text = Parcela.ValorParcela.ToString("C");
                lblVencimentoParcela.Text = Parcela.Vencimento.ToString("dd/MM/yyyy");
                lblSituacaoParcela.Text = DmoBase.DescricaoEnum<DmoParcela.SituacoesParcela>(Parcela.SituacaoParcela);

                switch (Parcela.SituacaoParcela)
                {
                    case DmoParcela.SituacoesParcela.EmAberto:
                        pnlSituacaoParcela.BackColor = Color.LightBlue;
                        break;
                    case DmoParcela.SituacoesParcela.PagoSemAtraso:
                        if (Parcela.DataDoPagamento != null)
                            lblSituacaoParcela.Text += " em " + Parcela.DataDoPagamento.ToString();
                        pnlSituacaoParcela.BackColor = Color.LightGreen;
                        break;
                    case DmoParcela.SituacoesParcela.PagoComAtraso:
                        pnlSituacaoParcela.BackColor = Color.LightYellow;
                        break;
                    case DmoParcela.SituacoesParcela.Cancelado:
                        pnlSituacaoParcela.BackColor = Color.LightPink;
                        break;
                }

                if(Parcela.SituacaoParcela == DmoParcela.SituacoesParcela.EmAberto)
                {
                    btnMudarSituacaoParcela.IconChar = FontAwesome.Sharp.IconChar.HandHoldingUsd;
                }
                else 
                {
                    btnMudarSituacaoParcela.IconChar = FontAwesome.Sharp.IconChar.TimesCircle;
                    btnMudarSituacaoParcela.Visible = false;
                }
            }
        }
        #endregion

        #region Evento
        private void ucParcelaVenda_Load(object sender, EventArgs e)
        {
            this.Width = this.Parent.Width - SystemInformation.VerticalScrollBarWidth - 20;
        }

        private void btnMudarSituacaoParcela_Click(object sender, EventArgs e)
        {
            if (MessageBox.Show($"Confirmar pagamento da Parcela { Parcela.Parcela } no valor de { Parcela.ValorParcela.ToString("C") } no dia { DateTime.Today.ToLongDateString() }?", "Confirmar pagamento da Parcela", MessageBoxButtons.YesNoCancel, MessageBoxIcon.Question) == DialogResult.Yes)
            {
                try
                {
                    DmoParcela parcelaPaga = Parcela;

                    if (DateTime.Today > Parcela.Vencimento)
                        parcelaPaga.SituacaoParcela = DmoParcela.SituacoesParcela.PagoComAtraso;
                    else
                        parcelaPaga.SituacaoParcela = DmoParcela.SituacoesParcela.PagoSemAtraso;

                    parcelaPaga.DataDoPagamento = DateTime.Now;

                    new BoParcela().AtualizarSituacaoParcela(int.Parse(Parcela.Venda.IdVenda.ToString()), Parcela.Parcela, parcelaPaga.SituacaoParcela, parcelaPaga.DataDoPagamento);


                    Parcela = parcelaPaga;
                }
                catch(Exception erro)
                {
                    MessageBox.Show("Aconteceu um erro ao tentar pagar parcela. Mensagem original: " + erro.Message, "Erro", MessageBoxButtons.RetryCancel, MessageBoxIcon.Error);
                }
            }
        }
        #endregion
    }
}