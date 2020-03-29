﻿using KadoshModas.DAL;
using KadoshModas.DML;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KadoshModas.BLL
{
    /// <summary>
    /// Classe BLL para Cidade
    /// </summary>
    class BoCidade
    {
        /// <summary>
        /// Consulta todas as Cidades de um determinado Estado
        /// </summary>
        /// <returns>Retorna uma lista de DmoCidade com todas as Cidades do Estado especificado</returns>
        public List<DmoCidade> ConsultarDoEstado(int pIdEstado)
        {
            return new DaoCidade().ConsultarDoEstado(pIdEstado);
        }
    }
}