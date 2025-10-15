using HotChocolate.Execution;
using System;
using System.Collections.Generic;

namespace Artigo.API.GraphQL.ErrorFilters
{
    public class ApplicationErrorFilter : IErrorFilter
    {
        public IError OnError(IError error)
        {
            return error.Exception switch
            {
                System.Collections.Generic.KeyNotFoundException knfe => error.WithCode("RESOURCE_NOT_FOUND").WithMessage(knfe.Message),
                InvalidOperationException ioe => error.WithCode("BUSINESS_INVALID_OPERATION").WithMessage(ioe.Message),
                _ => error.WithMessage("Ocorreu um erro interno de processamento."),
            };
        }
    }
}