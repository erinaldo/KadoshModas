﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace KadoshModas.INF
{
    /// <summary>
    /// Classe de infraestrutura que define os diretórios do sistema
    /// </summary>
    class DiretoriosDoSistema
    {
        /// <summary>
        /// Diretório aonde serão salvas as fotos dos clientes (caso não exista, será criado e seu caminho retornado em uma string)
        /// </summary>
        public static string DIR_FOTOS_CLIENTES
        {
            get { return Directory.CreateDirectory(Path.GetDirectoryName(Application.ExecutablePath) + "\\FotosClientes").FullName; }
        }
    }
}