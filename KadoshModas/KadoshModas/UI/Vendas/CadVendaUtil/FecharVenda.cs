﻿using KadoshModas.BLL;
using KadoshModas.DML;
using KadoshModas.UI.Vendas.CadVendaUtil;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace KadoshModas.UI
{
    public partial class FecharVenda : Form
    {
        #region Construtor
        public FecharVenda()
        {
            InitializeComponent();
        }

        public FecharVenda(DmoVenda pVenda)
        {
            InitializeComponent();
            this.Venda = pVenda;
            FecharVenda.SituacaoVenda = SituacoesVenda.Indefinido;
            AtualizarTotal();
        }
        #endregion

        #region Propriedades

        /// <summary>
        /// Propriedade Venda utilizada para realizar fechamento da Venda
        /// </summary>
        private DmoVenda Venda { get; set; }

        /// <summary>
        /// Propriedade estática que define se a Venda foi concluída com sucesso
        /// </summary>
        public static SituacoesVenda SituacaoVenda { get; set; }
        #endregion

        #region Enum
        /// <summary>
        /// Enum que define situações possíveis para a Venda
        /// </summary>
        public enum SituacoesVenda
        {
            [Description("Venda não Finalizada")]
            Indefinido,

            [Description("Venda Fechada com Sucesso")]
            Sucesso,

            [Description("Venda Cancelada")]
            VendaCancelada,

            [Description("Venda não Finalizada por conta de Erro")]
            Erro
        }
        #endregion

        #region Métodos
        /// <summary>
        /// Atualiza o Total da Venda e exibe o valor na tela
        /// </summary>
        private void AtualizarTotal()
        {
            float total = 0f;

            foreach (DmoItemDaVenda itemDaVenda in Venda.ItensDaVenda)
            {
                total += itemDaVenda.Valor * itemDaVenda.Quantidade;
            }

            total -= total * (Venda.Desconto / 100);
            Venda.Total = total;
            lblTotal.Text = total.ToString("C");

            //Atualizar entrada
            Venda.Entrada = string.IsNullOrEmpty(txtEntrada.Text) ? 0f : int.Parse(txtEntrada.Text.Replace(",", "."));
            
            if (Venda.Entrada >= total)
            {
                double novaEntrada = Math.Round(Venda.Total / 2f); // Nova entrada será metade do valor total arredondado
                MessageBox.Show($"Entrada não pode ser maior ou igual o Total da Venda! A entrada será alterada para {novaEntrada:C}", "Atenção!", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                txtEntrada.Text = novaEntrada.ToString().Replace(".", ",");
                Venda.Entrada = novaEntrada;
            }

            Venda.Pago = Venda.Entrada;
            lblEntrada.Text = Venda.Entrada.ToString("C");

            //Atualizar parcelas
            if (cboQtdParcelas.SelectedItem != null)
            {
                if (int.TryParse(cboQtdParcelas.SelectedItem.ToString(), out int qtdParcelas))
                {
                    Venda.ParcelasDaVenda = new List<DmoParcela>();
                    Venda.QtdParcelas = uint.Parse(qtdParcelas.ToString());
                    double valorDaParcela = (Venda.Total - Venda.Entrada) / Venda.QtdParcelas;

                    for (int i = 1; i <= Venda.QtdParcelas; i++)
                    {
                        Venda.ParcelasDaVenda.Add(new DmoParcela()
                        {
                            Parcela = i,
                            ValorParcela = valorDaParcela,
                            SituacaoParcela = SituacaoParcela.EmAberto,
                            Vencimento = DateTime.Now.AddMonths(i),
                            Desconto = 0
                        });
                    }

                    lblParcelas.Text = cboQtdParcelas.SelectedItem + "x de " + valorDaParcela.ToString("C");
                }
            }
        }

        /// <summary>
        /// Calcula o troco para pagamentos à vista e exibe na tela
        /// </summary>
        private void CalcularTroco()
        {
            if(float.TryParse(txtValorPago.Text, out float valorPago))
            {
                if (valorPago > Venda.Total)
                {
                    lblTroco.Text = (valorPago - Venda.Total).ToString("C");
                    return;
                }
            }

            lblTroco.Text = "R$ 0,00";
        }
        #endregion

        #region Eventos
        private void FecharVenda_Load(object sender, EventArgs e)
        {
            this.Icon = Properties.Resources.ICONE_KADOSH_128X128;

            cboFormaDePagamento.DataSource = new BindingSource(DmoVenda.DescricoesEnum<FormaDePagamento>().OrderBy(key => key.Value), null);
            cboFormaDePagamento.DisplayMember = "Key";

            Venda.FormaDePagamento = FormaDePagamento.Dinheiro;
            Venda.QtdParcelas = 1;
        }

        private void cboFormaDePagamento_SelectedIndexChanged(object sender, EventArgs e)
        {
            Venda.FormaDePagamento = (FormaDePagamento)((KeyValuePair<string, int>)cboFormaDePagamento.SelectedValue).Value;
            
            lblValorPagoRotulo.Visible = txtValorPago.Visible = lblTrocoRotulo.Visible = lblTroco.Visible = Venda.FormaDePagamento == FormaDePagamento.Dinheiro && !rbtAPrazo.Checked;
        }

        private void rbtAPrazo_CheckedChanged(object sender, EventArgs e)
        {
            if (cboQtdParcelas.SelectedIndex == -1)
                cboQtdParcelas.SelectedIndex = 0;

            lblPrazoEm.Visible = lblPrazoVezes.Visible = cboQtdParcelas.Visible = lblParcelasRotulo.Visible = lblParcelas.Visible = lblEntradaRotulo.Visible = txtEntrada.Visible = lblEntrada.Visible = rbtAPrazo.Checked;

            lblValorPagoRotulo.Visible = txtValorPago.Visible = lblTrocoRotulo.Visible = lblTroco.Visible = Venda.FormaDePagamento == FormaDePagamento.Dinheiro && !rbtAPrazo.Checked;
        }

        private void cboQtdParcelas_SelectedIndexChanged(object sender, EventArgs e)
        {
            AtualizarTotal();
        }

        private void trbDesconto_ValueChanged(object sender, EventArgs e)
        {
            lblDesconto.Text = trbDesconto.Value.ToString() + "%";

            if (trbDesconto.Value < 50)
                lblDesconto.ForeColor = Color.DarkGreen;
            else if (trbDesconto.Value < 80)
                lblDesconto.ForeColor = Color.Blue;
            else
                lblDesconto.ForeColor = Color.Red;

            Venda.Desconto = trbDesconto.Value;
            AtualizarTotal();
        }

        private void trbEntrada_ValueChanged(object sender, EventArgs e)
        {
            AtualizarTotal();
        }

        private async void btnFecharVenda_Click(object sender, EventArgs e)
        {
            try
            {
                if (rbtAVista.Checked)
                {
                    Venda.Situacao = DML.SituacaoVenda.Concluido;
                    Venda.TipoPagamento = TipoPagamento.AVista;
                }
                else if (rbtFiado.Checked)
                {
                    Venda.Situacao = DML.SituacaoVenda.EmAberto;
                    Venda.TipoPagamento = TipoPagamento.Fiado;
                }
                else if (rbtAPrazo.Checked)
                {
                    Venda.Situacao = DML.SituacaoVenda.EmAberto;
                    Venda.TipoPagamento = TipoPagamento.Parcelado;
                }

                await new BoVenda().CadastrarAsync(Venda);
                MessageBox.Show("Venda concluída com sucesso!", "Fechamento de Venda", MessageBoxButtons.OK, MessageBoxIcon.Information);
                FecharVenda.SituacaoVenda = SituacoesVenda.Sucesso;

                if (MessageBox.Show("Imprimir recibo da venda?", "Impressão de recibo.", MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes)
                {
                    new ImpressaoDeRecibo(
                        Venda.Cliente.Nome, 
                        Venda.DataVenda, 
                        Venda.Entrada,
                        Venda.Total,
                        Venda.Situacao == DML.SituacaoVenda.Concluido,
                        DmoItemDaVenda.ConverterItensParaRomaneio(Venda.ItensDaVenda)
                        ).ShowDialog();
                }

                this.Close();
            }
            catch (Exception erro)
            {
                MessageBox.Show("Aconteceu um erro ao Fechar a Venda. Mensagem original: " + erro.Message, "Não foi possível Cadastrar a Venda", MessageBoxButtons.OK, MessageBoxIcon.Error);
                FecharVenda.SituacaoVenda = SituacoesVenda.Erro;
                this.Close();
            }
        }

        private void btnCancelarFechamento_Click(object sender, EventArgs e)
        {
            FecharVenda.SituacaoVenda = SituacoesVenda.VendaCancelada;
            this.Close();
        }

        private void txtValorPago_KeyPress(object sender, KeyPressEventArgs e)
        {
            //Permitir somente caracteres numéricos
            if (!char.IsControl(e.KeyChar) && !char.IsDigit(e.KeyChar) && (e.KeyChar != ','))
            {
                e.Handled = true;
            }

            //Permitir somente uma vírgula
            if ((e.KeyChar == ',') && ((sender as TextBox).Text.IndexOf(',') > -1))
            {
                e.Handled = true;
            }
        }

        private void txtValorPago_TextChanged(object sender, EventArgs e)
        {
            CalcularTroco();
        }

        private void rbtAVista_CheckedChanged(object sender, EventArgs e)
        {
            if (rbtAVista.Checked)
            {
                Venda.QtdParcelas = 1;
                Venda.ParcelasDaVenda = new List<DmoParcela>();
            }
        }

        private void txtEntrada_KeyPress(object sender, KeyPressEventArgs e)
        {
            if (!char.IsControl(e.KeyChar) && !char.IsDigit(e.KeyChar) && (e.KeyChar != ','))
            {
                e.Handled = true;
            }

            // Permitir somente um caractere decimal
            if ((e.KeyChar == ',') && ((sender as TextBox).Text.IndexOf(',') > -1))
            {
                e.Handled = true;
            }
        }

        private void txtEntrada_TextChanged(object sender, EventArgs e)
        {
            AtualizarTotal();
        }
        #endregion

        private void rbtFiado_CheckedChanged(object sender, EventArgs e)
        {
            lblEntradaRotulo.Visible = txtEntrada.Visible = lblEntrada.Visible = rbtFiado.Checked;
        }
    }
}