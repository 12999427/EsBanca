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
            Console.WriteLine("Persona {0} con metallo={1} entra in attesa. Attesa totale: {2} ora i contatori sono: in cabina={3}, in interno={4}, in attesa={5}", Id, HaMetallo, Banca.N_Attesa, Banca.N_Cabina, Banca.N_Interno, Banca.N_Attesa);

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
            Console.WriteLine("In attesa... " + Banca.S_Interno.CurrentCount);
            await Banca.S_Interno.WaitAsync();

            if (Banca.ControllaTuttiGiustiInCabina())
            {

                await Banca.S_Mutex_Var.WaitAsync();
                    Banca.N_Cabina -= 1;
                    Banca.L_Cabina.Remove(this);
                    Banca.N_Interno += 1;
                    Banca.L_Interno.Add(this);
                Banca.S_Mutex_Var.Release();
                
                Console.WriteLine("Persona {0} con metallo={1} entra nella banca e fa le sue robe!", Id, HaMetallo);
                
                await Task.Delay(Banca.random.Next(500, 2000)); //tempo dentro la banca

                await Banca.S_Mutex_Var.WaitAsync();
                    Banca.L_Interno.Remove(this);
                Banca.S_Mutex_Var.Release();
                //Console.WriteLine("asdasd");
                await Banca.SbloccaIngressoStanzaSeAumentato();
                //Console.WriteLine(Banca.S_Cabina.CurrentCount);
            }
            else
            {
                await Banca.S_Mutex_Var.WaitAsync();
                    Banca.L_Cabina.Remove(this);
                Banca.S_Mutex_Var.Release();
                await Banca.SbloccaIngressoStanzaSeAumentato();

                Console.WriteLine("Persona {0} con metallo={1} viene cacciata fuori!", Id, HaMetallo);
            }

            Console.WriteLine("Persona {0} con metallo={1} esce dalla cabina e dalla banca.", Id, HaMetallo);
            //Banca.S_Cabina.Release();
        }
    }
}
