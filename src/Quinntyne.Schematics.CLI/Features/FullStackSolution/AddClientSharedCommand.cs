using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Quinntyne.Schematics.Infrastructure.Interfaces;
using Quinntyne.Schematics.Infrastructure.Services;
using MediatR;
using FluentValidation;
using System.Linq;
using System;

namespace Quinntyne.Schematics.CLI.Features.FullStackSolution
{
    public class AddClientSharedCommand
    {
        public class Request : Options, IRequest, ICodeGeneratorCommandRequest
        {
            public Request(IOptions options)
            {
                Entity = options.Entity;
                Directory = options.Directory;
                Namespace = options.Namespace;
                RootNamespace = options.RootNamespace;
                Options = options;
            }

            public dynamic Settings { get; set; }
            public IOptions Options { get; set; }
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
            private readonly IMediator _mediator;

            public Handler(
                IMediator mediator
                )
            {
                _mediator = mediator;
            }

            public async Task Handle(Request request, CancellationToken cancellationToken)
            {

                var names = string.Join(",", new List<string> {
                    "shared.module.ts",                    
                    });

                await _mediator.Send(new GenerateFileCommand.Request(request.Options)
                {
                    Name = names,
                    SolutionDirectory = request.SolutionDirectory
                });
            }
        }
    }
}
