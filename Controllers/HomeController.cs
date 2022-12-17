using CsvHelper;
using Microsoft.AspNetCore.Mvc;
using RHManager.Models;
using RHManager.Util;
using System.Diagnostics;
using static RHManager.Models.FileViewModel;
using System.Globalization;
using System.Text;
using Newtonsoft.Json;
using TinyCsvParser.Reflection;
using System.Text.Json;
using JsonSerializer = System.Text.Json.JsonSerializer;

namespace RHManager.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        public HomeController(ILogger<HomeController> logger)
        {
            _logger = logger;
        }

        public IActionResult Index(string? errorMessage)
        {
            ViewBag.ErrorMessage = errorMessage;
            return View();
        }

        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }

        public IActionResult About()
        {
            return View();
        }


        [HttpPost]
        public async Task<IActionResult> ReadFiles(string pasta)
        {
            try
            {
                /*Ler e pegar os arquivos dentro da pasta indicada*/
                DirectoryInfo folder = new DirectoryInfo(pasta);
                List<string> extensions = new List<string>() { ".csv" };
                List<FileInfo> files = folder.EnumerateFiles()
                                        .Where(s => extensions.Contains(s.Extension))
                                        .ToList();

                /* Converter os dados dos arquivos .csv em objetos JSON */
                var departamentos = await GetDepartamentos(files);
                var jsonstr = JsonSerializer.Serialize(departamentos);
                byte[] byteArray = ASCIIEncoding.ASCII.GetBytes(jsonstr);
                return File(byteArray, "application/force-download", "Saldo_Departamentos.json");
            }
            catch
            {
                return RedirectToAction("Index", new { errorMessage = "O diretorio esta vazio ou não existe" } );
            }

        }

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
            catch (Exception ex)
            {
                return null;
            }

            return departamentos;
        }



    }
}