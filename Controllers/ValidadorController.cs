using Microsoft.AspNetCore.Mvc;
using System;
using System.Linq;

namespace ValidadorCpfCnpjApi1.Controllers
{
    // Define the route for this controller / Define a rota para este controlador
    [Route("api/[controller]")]
    // Indicates that this is an API controller / Indica que este é um controlador de API
    [ApiController]
    public class ValidadorController : ControllerBase
    {
        // HTTP POST method that validates the given document / Método HTTP POST que valida o documento fornecido
        [HttpPost("validar")]
        public IActionResult Validar([FromBody] ValidacaoRequest request)
        {
            string documento = request.Documento;

            // Remove unnecessary characters / Remove caracteres desnecessários
            string documentoLimpo = documento.Trim().Replace(".", "").Replace("-", "").Replace("/", "").Replace("%2F", "");

            // Log para verificar o documento após a limpeza
            Console.WriteLine($"Documento após limpeza: {documentoLimpo}");

            if (documentoLimpo.Length == 11 && IsCpf(documentoLimpo))
            {
                return Ok("CPF válido");
            }
            // Check if the document is a CNPJ / Verifica se o documento é um CNPJ
            else if (documentoLimpo.Length == 14 && IsCnpj(documentoLimpo))
            {
                return Ok("CNPJ válido");
            }
            else
            {
                return BadRequest("Documento inválido");
            }
        }

        // Method to validate CPF / Método para validar CPF
        private bool IsCpf(string cpf)
        {
            // Remove unnecessary characters / Remove caracteres desnecessários
            cpf = cpf.Trim().Replace(".", "").Replace("-", "").Replace("/", "").Replace("%2F", "");

            // Check the length and if all digits are the same / Verifica o comprimento e se todos os dígitos são iguais
            if (cpf.Length != 11 || cpf.All(c => c == cpf[0]))
                return false;

            int[] digits = cpf.Select(c => int.Parse(c.ToString())).ToArray();
            int sum = 0, weight;

            // Validate the first digit / Valida o primeiro dígito
            weight = 10;
            for (int i = 0; i < 9; i++)
            {
                sum += digits[i] * weight--;
            }

            int remainder = (sum * 10) % 11;
            if (remainder == 10) remainder = 0;
            if (remainder != digits[9]) return false;

            // Validate the second digit / Valida o segundo dígito
            sum = 0;
            weight = 11;
            for (int i = 0; i < 10; i++)
            {
                sum += digits[i] * weight--;
            }

            remainder = (sum * 10) % 11;
            if (remainder == 10) remainder = 0;
            return remainder == digits[10];
        }

        // Method to validate CNPJ / Método para validar CNPJ
        private bool IsCnpj(string cnpj)
        {
            cnpj = cnpj.Trim().Replace(".", "").Replace("-", "").Replace("/", "");

            if (cnpj.Length != 14 || cnpj.All(c => c == cnpj[0]))
                return false;

            int[] digits = cnpj.Select(c => int.Parse(c.ToString())).ToArray();
            int[] weights1 = { 5, 4, 3, 2, 9, 8, 7, 6, 5, 4, 3, 2 };
            int[] weights2 = { 6, 5, 4, 3, 2, 9, 8, 7, 6, 5, 4, 3, 2 };

            int sum = 0;
            for (int i = 0; i < 12; i++)
            {
                sum += digits[i] * weights1[i];
            }

            int remainder = (sum % 11);
            if (remainder < 2)
            {
                if (digits[12] != 0) return false;
            }
            else
            {
                if (digits[12] != 11 - remainder) return false;
            }

            sum = 0;
            for (int i = 0; i < 13; i++)
            {
                sum += digits[i] * weights2[i];
            }

            remainder = (sum % 11);
            if (remainder < 2)
            {
                if (digits[13] != 0) return false;
            }
            else
            {
                if (digits[13] != 11 - remainder) return false;
            }

            return true;
        }
    }

    public class ValidacaoRequest
    {
        public string Documento { get; set; }
    }
}



