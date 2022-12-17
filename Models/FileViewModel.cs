using System.Globalization;
using System;
using System.Security.Cryptography.X509Certificates;
using CsvHelper.Configuration;
using CsvHelper.Configuration.Attributes;
using RHManager.Util;

namespace RHManager.Models
{
    public class FileViewModel
    {
        public class DepartamentoModel
        {
            public string Departamento { get; set; }
            public string MesVigencia { get; set; }
            public string AnoVigencia { get; set; }
            public decimal TotalPagar { get; set; }
            public decimal TotalDescontos { get; set; }
            public decimal TotalExtras { get; set; }
            public List<FuncionarioModel> Funcionarios { get; set; }


            public void setValoresMensais()
            {
                foreach (var funcionario in Funcionarios)
                {
                    TotalPagar += funcionario.ValorHorasTrabalho;
                    TotalExtras += funcionario.ValorHorasExtras;
                    TotalDescontos += funcionario.ValorHorasDescontos;
                }
            }
        }

        public class FuncionarioModel
        {
            public string Nome { get; set; }
            public int Codigo { get; set; }

            public decimal ValorHora { get; set; }

            public decimal ValorHorasTrabalho { get; set; }
            public decimal ValorHorasExtras { get; set; }
            public decimal ValorHorasDescontos { get; set; }

            public int DiasFalta { get; set; }
            public int DiasExtra { get; set; }
            public int DiasTrabalhados { get; set; }


            public void setValorDias(int diasDeTrabalho, string mes, int ano)
            {
                DiasTrabalhados = diasDeTrabalho;

                int month = DateTime.ParseExact(mes, "MMMM", CultureInfo.CurrentCulture).Month;
                int diasDeSemana = CustomConverter.GetWeekDaysOfMonth(month, ano);

                if(diasDeTrabalho > diasDeSemana)
                {
                    DiasExtra = diasDeTrabalho - diasDeSemana;
                }
                if(diasDeTrabalho < diasDeSemana)
                {
                    DiasFalta = diasDeSemana - diasDeTrabalho;
                }

            }
            public void setValorHorasTrabalhadas(double horasTrabalhadas)
            {
                if (horasTrabalhadas >= 8)
                {
                    if (horasTrabalhadas > 8)
                    {
                        ValorHorasExtras += (decimal)(horasTrabalhadas - 8f) * ValorHora;
                        horasTrabalhadas = horasTrabalhadas - (horasTrabalhadas - 8);
                    }

                    ValorHorasTrabalho += (decimal)horasTrabalhadas * ValorHora;
                }

                if (horasTrabalhadas < 8)
                {
                    ValorHorasDescontos += (decimal)(horasTrabalhadas - 8) * -1;
                }
            }
        }

        public class FileDefault
        {
            [Name("Codigo")]
            public int Codigo { get; set; }
            [Name("Nome")]
            public string Nome { get; set; }
            [Name("Valor Hora")]
            public string ValorHora { get; set; }
            [Name("Data")]
            public string Data { get; set; }
            [Name("Entrada")]
            public string Entrada { get; set; }
            [Name("Saida")]
            public string Saida { get; set; }
            [Name("Almoço")]
            public string almoco { get; set; }

        }


        /* Classe comentada pela falta de necessidade mas, 
         * esta aqui caso seja necessaria futuramente
         
        public class DiaTrabalho{
            public DateTime Entrada { get; set; }
            public DateTime Saida { get; set; }
            public float Almoco { get; set; }
            public double TotalHoras()
            {
                return ((double)(Saida - Entrada).TotalHours) - (double)Almoco;
            }
        }
        */
    }

}
