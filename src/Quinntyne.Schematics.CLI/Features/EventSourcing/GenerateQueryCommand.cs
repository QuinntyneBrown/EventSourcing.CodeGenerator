using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Quinntyne.Schematics.Infrastructure.Interfaces;
using Quinntyne.Schematics.Infrastructure.Services;
using MediatR;
using FluentValidation;

namespace Quinntyne.Schematics.CLI.Features.EventSourcing
{
    public class GenerateQueryCommand
    {
        public class Request: Options, IRequest, ICodeGeneratorCommandRequest
        {
            public Request(IOptions options)
            {                
                Entity = options.Entity;
                Name = options.Name;
                Directory = options.Directory;
                Namespace = options.Namespace;
                RootNamespace = options.RootNamespace;
            }

            public dynamic Settings { get; set; }
        }

        public class Validator : AbstractValidator<Request>
        {
            public Validator()
            {
                RuleFor(request => request.Entity).NotNull();
            }
        }

        public class Handler : IRequestHandler<Request>
        {
            private readonly IFileWriter _fileWriter;
            private readonly ITemplateLocator _templateLocator;
            private readonly ITemplateProcessor _templateProcessor;
            private readonly INamingConventionConverter _namingConventionConverter;

            public Handler(
                IFileWriter fileWriter,
                INamingConventionConverter namingConventionConverter,
                ITemplateLocator templateLocator, 
                ITemplateProcessor templateProcessor
                )
            {
                _fileWriter = fileWriter;
                _namingConventionConverter = namingConventionConverter;
                _templateProcessor = templateProcessor;
                _templateLocator = templateLocator;
            }

            public Task Handle(Request request, CancellationToken cancellationToken)
            {                
                var entityNamePascalCase = _namingConventionConverter.Convert(NamingConvention.PascalCase, request.Entity);
                var entityNameCamelCase = _namingConventionConverter.Convert(NamingConvention.CamelCase, request.Entity);
                var entityNamePascalCasePlural = _namingConventionConverter.Convert(NamingConvention.PascalCase, request.Entity, true);
                var entityNameCamelCasePlural = _namingConventionConverter.Convert(NamingConvention.CamelCase, request.Entity, true);
                var namePascalCase = _namingConventionConverter.Convert(NamingConvention.PascalCase, request.Name);

                var template = _templateLocator.Get("GenerateQueryCommand");

                var tokens = new Dictionary<string, string>
                {
                    { "{{ entityNamePascalCase }}", entityNamePascalCase },
                    { "{{ entityNameCamelCase }}", entityNameCamelCase },
                    { "{{ namespace }}", request.Namespace },
                    { "{{ rootNamespace }}", request.RootNamespace },
                    { "{{ namePascalCase }}", namePascalCase },
                    { "{{ entityNamePascalCasePlural }}", entityNamePascalCasePlural },
                    { "{{ entityNameCamelCasePlural }}", entityNameCamelCasePlural },
                };

                var result = _templateProcessor.ProcessTemplate(template, tokens);
                
                _fileWriter.WriteAllLines($"{request.Directory}//Get{namePascalCase}Query.cs", result);
               
                return Task.CompletedTask;
            }
        }
    }
}
