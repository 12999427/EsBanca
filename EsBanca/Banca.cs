using System;
using System.Collections.Generic;
using System.Security.Cryptography;
using System.Threading;
using System.Threading.Tasks;

namespace EsBanca
{
    class Banca
    {
        public SemaphoreSlim S_Cabina { get; private set; } //rappresenta il numero di posti
        // liberi nella cabina | coloro che sono in attesa (wait async) sono in attesa perchè si liberi la cabina
        // Da esso si può anche capire se la porta è rivolta verso l'esterno o l'interno
        public int N_Cabina; //equivale a L_Cabina.Count
        public List<Persona> L_Cabina;


        public SemaphoreSlim S_Interno { get; private set; } //rappresenta l'interno della banca 
        public int N_Interno; //equivale a L_Interno.Count
        public List<Persona> L_Interno;

        public SemaphoreSlim S_Mutex_Var { get; private set; } //mutex per la protezione delle variabili condivise ^^


        public int N_Attesa;
        public List<Persona> L_Attesa;

        //public bool PortaRivoltaEsterno;
        public int PostiCabina;
        public Random random;


        public Banca(int PostiCabina=5)
        {
            S_Mutex_Var = new(1, 1);

            this.PostiCabina = PostiCabina;
            this.random = new Random();

            S_Cabina = new(PostiCabina, PostiCabina);
            N_Cabina = 0;
            S_Interno = new(0);
            N_Interno = 0;
            N_Attesa = 0;
            L_Attesa = new();
            L_Cabina = new();
            L_Interno = new();

            //PortaRivoltaEsterno = true;
        }

        public async Task ControllaSeSbloccareIngressoStanza()
        {
            await S_Mutex_Var.WaitAsync();
                if (N_Cabina == PostiCabina || N_Attesa == 0)
                {
                    if (N_Cabina < PostiCabina)
                    {
                        int diff = PostiCabina - N_Cabina;
                        N_Cabina += diff;
                        for (int i = 0; i<diff; i++)
                        {
                            S_Cabina.Wait();
                        }
                    }
                    //PortaRivoltaEsterno = false;
                    S_Interno.Release(PostiCabina);
                    Console.WriteLine("Banca: cabina piena o nessun'altro in attesa, sblocco ingresso stanza.");
                }
            S_Mutex_Var.Release();
        }

        public async Task SbloccaIngressoStanzaSeAumentato()
        {
            await S_Mutex_Var.WaitAsync();
                for (int i = 0; i<N_Cabina; i++)
                {
                    S_Cabina.Release();
                }
                Console.WriteLine("Riporto: " + N_Cabina);
                N_Cabina = 0;
            S_Mutex_Var.Release();
        }

        public bool ControllaTuttiGiustiInCabina()
        {
            
            bool trovatoMetallo = false;
            foreach (Persona p in L_Cabina)
            {
                if (p.HaMetallo)
                {
                    trovatoMetallo = true;
                    break;
                }
            }

            return !trovatoMetallo;
        }
    }
}
