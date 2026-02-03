using System;
using System.Collections.Generic;
using System.Reflection.Metadata;
using System.Threading;
using System.Threading.Tasks;

namespace EsBanca
{
    internal class Program
    {
        static int numPersone = 3;
        static async Task Main(string[] args)
        {
            Random random = new();

            Banca banca = new(3);

            List<Persona> persone = new();
            List<Task> tasks = new();
            for (int i = 0; i<numPersone; i++)
            {
                persone.Add(new Persona(i, random.Next(0, 5) == 0, banca));
                tasks.Add(persone[i].EntraInBanca());
                //await Task.Delay(random.Next(200, 500));
            }

            await Task.WhenAll(tasks);
        }
    }
}
