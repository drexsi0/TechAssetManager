using GerenciadorAtivos.Models;
using Xunit;

namespace GerenciadorAtivos.Tests
{
    public class AtivoTests
    {
        [Fact]
        public void ValorAtual_ReduzAproximadamenteVintePorCentoAoAno()
        {
            var ativo = new Ativo
            {
                ValorCompra = 1000m,
                DataCompra = DateTime.Now.AddYears(-1)
            };

            Assert.InRange(ativo.ValorAtual, 790m, 810m);
        }

        [Fact]
        public void ValorAtual_NuncaFicaNegativo()
        {
            var ativo = new Ativo
            {
                ValorCompra = 1000m,
                DataCompra = DateTime.Now.AddYears(-10)
            };

            Assert.Equal(0m, ativo.ValorAtual);
        }
    }
}
