# RH MANAGER
Sistema Web feito no .NET 6 (C#), no padrão MVC.
Dedicado para a leitura de arquivos .csv com uma montagem especifica para arquivos JSON.

Função principal do sistema pode ser acessado na tela em si, que irá baixar um arquivo JSON com os dados desejados,
ou via o metodo POST 'api/file/getfile' com a string do caminho desejado no body (repito, uma string, não um json), e o retorno será o mesmo JSON.

A pasta FilesRH contem alguns varios arquivos .csv para teste;