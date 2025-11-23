using System;

namespace Usuario.Intf.Models
{
    // DTO genérico para comunicar o resultado de uma operação complexa (como envio de e-mail ou validação de token)
    // entre o Service e o Controller. Ele substitui a necessidade de lançar exceções para erros de negócio.
    public class ServiceResult 
    {
        // Indica se a operação foi concluída com sucesso.
        public bool IsSuccess { get; set; }

        // Mensagem de retorno (sucesso ou erro detalhado).
        public string Message { get; set; } = string.Empty;

        // Propriedade opcional para um código de erro HTTP específico, se necessário.
        public int StatusCode { get; set; } = 200;

        // Método estático para retornar sucesso facilmente.
        public static ServiceResult Success(string message = "Operação concluída com sucesso.", int statusCode = 200)
      => new ServiceResult { IsSuccess = true, Message = message, StatusCode = statusCode };

        // Método estático para retornar falha facilmente.
        public static ServiceResult Failure(string message, int statusCode = 400)
      => new ServiceResult { IsSuccess = false, Message = message, StatusCode = statusCode };
    }
}
