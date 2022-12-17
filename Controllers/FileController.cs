using CsvHelper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using RHManager.Util;
using System.Globalization;
using static RHManager.Models.FileViewModel;
using static System.Net.Mime.MediaTypeNames;

namespace RHManager.Controllers
{
    /* Caso o desejado seja uma API REST*/
    [Route("api/[controller]")]
    [ApiController]
    public class FileController : ControllerBase
    {
        [HttpPost("GetFile")]
        [AllowAnonymous]
        public async Task<ActionResult> GetFile([FromBody] string path)
        {
            try
            {
                DirectoryInfo folder = new DirectoryInfo(path);
                List<string> extensions = new List<string>() {".csv"};
                List<FileInfo> files = folder.EnumerateFiles()
                                        .Where(s => extensions.Contains(s.Extension))
                                        .ToList();
                  
                if (files.Count > 0)
                {
                    var departamentos = GetDepartamentos(files);
                    if(departamentos != null)
                        return Ok(departamentos);

                    return BadRequest("Ocorreu algum erro...");
                }

                return Ok("O diretorio esta vazio ou não existe");
            }
            catch (Exception e)
            {
                return BadRequest(e.Message);
            }

        }


        /*
          Função recebe todos os files .csv de um certo directory ou folder
            E retorna todos os dados de forma serializada para geração do arquivo JSON
         */
        public async Task<List<DepartamentoModel>> GetDepartamentos(List<FileInfo> files)
        {
            #region declarações
            List<DepartamentoModel> departamentos = new List<DepartamentoModel>();
            List<FuncionarioModel> funcionarios;
            FuncionarioModel funcionario;
            #endregion

            try
            {
                foreach (FileInfo file in files)
                {
                    int diasTrabalhados = 0;
                    funcionario = new FuncionarioModel();
                    funcionarios = new List<FuncionarioModel>();

                    using (var streamReader = new StreamReader(file.FullName))
                    {
                        using (var reader = new CsvReader(streamReader, CultureInfo.InvariantCulture))
                        {
                            var valores = reader.GetRecords<FileDefault>().OrderBy(item => item.Codigo).ToList();
                            foreach (var valor in valores)
                            {
                                /*
                                 Tratamento para adicionar funcionario na lista de funcionarios desse
                                 departamento caso o proximo item seja um novo funcionario ou o ultimo da lista
                                */
                                if ((funcionario.Codigo != valor.Codigo) || valor.Equals(valores.Last()))
                                {
                                    if (funcionario.Codigo != 0)
                                    {
                                        funcionario.setValorDias(diasTrabalhados, file.Name.Split('-')[1], int.Parse(file.Name.Split('-')[2].Split(".")[0]));
                                        funcionarios.Add(funcionario);

                                        if (valor.Equals(valores.Last()))
                                            continue;

                                        diasTrabalhados = 0;
                                    }

                                    funcionario = new FuncionarioModel()
                                    {
                                        Codigo = valor.Codigo,
                                        Nome = valor.Nome,
                                        ValorHora = decimal.Parse(valor.ValorHora, NumberStyles.Currency,
                                                                    CultureInfo.CurrentCulture.NumberFormat)
                                    };
                                }

                                /*Total de horas Trabalhadas((Saida - Entrada) - Horas de almoço)
                                 */
                                var totalHoras = ((float)(CustomConverter.StringToTime(valor.Saida) - CustomConverter.StringToTime(valor.Entrada)).TotalHours)
                                                   - ((float)(CustomConverter.StringToTime(valor.almoco.Split("-")[1].Trim() + ":00") - CustomConverter.StringToTime(valor.almoco.Split("-")[0].Trim() + ":00")).TotalHours);
                                funcionario.setValorHorasTrabalhadas(totalHoras);
                                diasTrabalhados++;
                            }
                        }
                    }
                    departamentos.Add(new DepartamentoModel()
                    {
                        Departamento = file.Name.Split('-')[0],
                        MesVigencia = file.Name.Split('-')[1],
                        AnoVigencia = file.Name.Split('-')[2].Split(".")[0],
                        Funcionarios = funcionarios
                    });
                }

                foreach (var depar in departamentos)
                {
                    depar.setValoresMensais();
                }

            } 
            catch(Exception ex) {
                return null;
            }

            return departamentos;
        }
        

        
        
    }

}
