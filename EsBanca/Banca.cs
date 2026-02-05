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
            Console.WriteLine("a");
            await S_Mutex_Var.WaitAsync();
                if (N_Cabina == PostiCabina || N_Attesa == 0)
                {
                    if (N_Cabina < PostiCabina)
                    {
                        int diff = PostiCabina - N_Cabina;
                        S_Mutex_Var.Release();
                        for (int i = 0; i<diff; i++)
                        {
                            S_Cabina.Wait();
                        }
                    }
                    else
                        S_Mutex_Var.Release();
                    //PortaRivoltaEsterno = false;
                    S_Interno.Release(N_Cabina);
                    Console.WriteLine("Banca: cabina piena o nessun'altro in attesa, sblocco ingresso stanza. ora i contatori sono: in cabina={0}, in interno={1}, in attesa={2} | Interno cur: {3} | Cabina cur: {4}", N_Cabina, N_Interno, N_Attesa, S_Interno.CurrentCount, S_Cabina.CurrentCount);
                }
                else
                    S_Mutex_Var.Release();

            Console.WriteLine("b");
        }

        public async Task SbloccaIngressoStanzaSeAumentato(bool esetiDaFerro)
        {
            await S_Mutex_Var.WaitAsync();
                if (esetiDaFerro)
                {
                    if (N_Interno != 0)
                    {
                        S_Cabina.Release(PostiCabina);
                        N_Interno = 0;
                    }
                }
                else
                {
                    if (N_Cabina != 0)
                    {
                        S_Cabina.Release(PostiCabina);
                        N_Cabina = 0;
                    }
                }
                Console.WriteLine("uscita dalla banca. ora i contatori sono: in cabina={0}, in interno={1}, in attesa={2} | Interno cur: {3} | Cabina cur: {4}", N_Cabina, N_Interno, N_Attesa, S_Interno.CurrentCount, S_Cabina.CurrentCount);
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
