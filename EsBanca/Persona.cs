using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace EsBanca
{
    class Persona
    {
        public bool HaMetallo { get; private set; }
        public int Id { get; private set; }
        private Banca Banca;

        public Persona(int id, bool haMetallo, Banca banca)
        {
            Id = id;
            HaMetallo = haMetallo;
            Banca = banca;
        }

        public async Task EntraInBanca()
        {
            await Banca.S_Mutex_Var.WaitAsync();
                Banca.N_Attesa += 1;
                Banca.L_Attesa.Add(this);
            Banca.S_Mutex_Var.Release();
            Console.WriteLine("Persona {0} con metallo={1} entra in attesa. Attesa totale: {2}", Id, HaMetallo, Banca.N_Attesa);

            await Banca.S_Cabina.WaitAsync();

            await Banca.S_Mutex_Var.WaitAsync();
                Banca.N_Cabina += 1;
                Banca.L_Cabina.Add(this);
                Banca.N_Attesa -= 1;
                Banca.L_Attesa.Remove(this);
            Banca.S_Mutex_Var.Release();
            Console.WriteLine("Persona {0} con metallo={1} entra in cabina. In cabina: {2}", Id, HaMetallo, Banca.N_Cabina);

            Console.WriteLine("Persona {0} con metallo={1} attende che la cabina si riempia.", Id, HaMetallo);

            await Banca.ControllaSeSbloccareIngressoStanza();

            //chiede alla banca se può entrare
            await Banca.S_Interno.WaitAsync();


            await Banca.S_Mutex_Var.WaitAsync();
                Banca.N_Cabina -= 1;
                Banca.L_Cabina.Remove(this);
            Banca.S_Mutex_Var.Release();


            if (Banca.ControllaTuttiGiustiInCabina())
            {
                Console.WriteLine("Persona {0} con metallo={1} entra nella banca e fa le sue robe!", Id, HaMetallo);
                await Task.Delay(Banca.random.Next(500, 2000)); //tempo dentro la banca
            }
            else
            {
                Console.WriteLine("Persona {0} con metallo={1} viene cacciata fuori!", Id, HaMetallo);
            }

            Console.WriteLine("Persona {0} con metallo={1} esce dalla cabina e dalla banca.", Id, HaMetallo);
            await Banca.SbloccaIngressoStanzaSeAumentato();
            Banca.S_Cabina.Release();
        }
    }
}
