using Crosspoint.Communicator;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Crosspoint.Api.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class CrosspointController : ControllerBase
    {

        private readonly ILogger<CrosspointController> logger;
        private readonly ExtronCrosspointCommunicator communicator;

        private readonly Dictionary<string, string> inputMappings;
        private readonly Dictionary<string, string> outputMappings;

        public CrosspointController(
            ILogger<CrosspointController> logger,
            IConfiguration configuration,
            ExtronCrosspointCommunicator communicator)
        {
            this.logger = logger;
            this.communicator = communicator;
            this.communicator.WaitUntilReady();
            this.inputMappings = new Dictionary<string, string>();
            this.outputMappings = new Dictionary<string, string>();

            configuration.GetSection("Extron").GetSection("Inputs").Bind(this.inputMappings);
            configuration.GetSection("Extron").GetSection("Outputs").Bind(this.outputMappings);
        }

        [HttpGet]
        public IEnumerable<Models.CrosspointMapping> Get() =>
            communicator.Mappings.Select(mapping =>
            {
                return new Models.CrosspointMapping
                {
                    AudioInput = mapping.Value.AudioInput,
                    VideoInput = mapping.Value.VideoInput,
                    Output = mapping.Key,
                };
            });

        [HttpGet("inputs")]
        public Dictionary<string, string> GetInputs()
        {
            var inputs = new Dictionary<string, string>();
            for(var input = 1; input <= communicator.Inputs; input++)
            {
                var inputKey = input.ToString();
                if (!inputMappings.TryGetValue(inputKey, out var name))
                {
                    name = $"Input {inputKey}";
                }
                inputs.Add(inputKey, name);
            }
            return inputs;
        }

        [HttpGet("outputs")]
        public Dictionary<string, string> GetOutputs()
        {
            var outputs = new Dictionary<string, string>();
            for (var output = 1; output <= communicator.Outputs; output++)
            {
                var outputKey = output.ToString();
                if (!outputMappings.TryGetValue(outputKey, out var name))
                {
                    name = $"Output {outputKey}";
                }
                outputs.Add(outputKey, name);
            }
            return outputs;
        }



        [HttpPost("{input}/{output}/{type}")]
        public IActionResult Post(int input, int output, int type)
        {
            if (!Enum.TryParse<MappingTypeEnum>(type.ToString(), out var mappingType))
            {
                return BadRequest($"{type} is not a valid mapping type");
            }
            communicator.MapInputToOutput(input, output, mappingType);
            return Ok();
        }

    }
}
