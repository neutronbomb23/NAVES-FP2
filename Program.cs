﻿//Francisco Miguel Galvan Muñoz
//Dorjee Khampa Herrezuelo Blasco
using System;
using System.Threading;
using System.Diagnostics;
using System.Text;
using System.IO;
using System.Drawing;

namespace naves
{

	class Program
	{
		static Random rnd = new Random(); // un único generador de aleaotorios para todo el programa
		const bool DEBUG = true; // para sacar información adicional en el Render

		const int ANCHO = 27, ALTO = 15,  // área de juego
				   MAX_BALAS = 5, MAX_ENEMIGOS = 9;

		struct Tunel
		{
			public int[] suelo, techo;
			public int ini;
		}

		struct Entidad
		{
			public int fil, col;
		}

		struct GrEntidades
		{
			public Entidad[] ent;
			public int num;
		}

        static void guarda(Entidad nave, GrEntidades balas, Tunel tunel, GrEntidades enemigos)
        {

            StreamWriter salida = new StreamWriter("saved.txt");
            salida.WriteLine(nave.col + " " + nave.fil);



			salida.WriteLine(balas.num);
            for (int j = 0; j < balas.num; j++)
            {
				salida.WriteLine(balas.ent[j].col + " " + balas.ent[j].fil);
            }


            salida.WriteLine(enemigos.num);
            for (int e = 0; e < enemigos.num; e++)
            {
                salida.WriteLine(enemigos.ent[e].col + " " + enemigos.ent[e].fil);
            }


            for (int k = 0; k < ANCHO; k++)
			{
				salida.WriteLine(tunel.suelo[k] + " " + tunel.techo[k]);
            }
				salida.WriteLine(tunel.ini);
                salida.Close();
        }

		static void carga(Entidad nave, ref GrEntidades balas, Tunel tunel, ref GrEntidades enemigos)
		{

			StreamReader entrada = new StreamReader("saved.txt");
			string[] linea = new string[2];
			linea = entrada.ReadLine().Split(' ');
			balas.num = int.Parse(entrada.ReadLine());

            for (int j = 0; j < balas.num; j++)
            {
                linea = entrada.ReadLine().Split(' ');
				balas.ent[j].col = int.Parse(linea[0]);
                balas.ent[j].fil = int.Parse(linea[1]);
            }



            enemigos.num = int.Parse(entrada.ReadLine());
         

            for (int e = 0; e < enemigos.num; e++)
            {
                linea = entrada.ReadLine().Split(' ');
                enemigos.ent[e].col = int.Parse(linea[0]);
                enemigos.ent[e].fil = int.Parse(linea[1]);
               
            }
            for (int k = 0; k < ANCHO; k++)
            {
                linea = entrada.ReadLine().Split(' ');
				tunel.suelo[k] = int.Parse(linea[0]);
                tunel.techo[k] = int.Parse(linea[1]);
				
            }

			tunel.ini = int.Parse(entrada.ReadLine());
            entrada.Close();
        }


        static void Main() 
		{
			Console.CursorVisible = false;

			Tunel tunel = new Tunel();
			Entidad nave = new Entidad();
			nave.col = ANCHO / 2;
			nave.fil = ALTO / 2;
			tunel.ini = 5;

			GrEntidades enemigos= new GrEntidades();
			enemigos.ent = new Entidad[MAX_ENEMIGOS];
			enemigos.num= 0;

			GrEntidades balas = new GrEntidades();
			balas.ent = new Entidad[MAX_BALAS];
			balas.num = 0;

			GrEntidades colisiones = new GrEntidades();
			colisiones.ent = new Entidad[MAX_BALAS + 1];
			colisiones.num = 0;

			
			
			IniciaTunel(out tunel);
			Console.WriteLine("Pulse H para cargar partida.");

			char s = char.Parse(Console.ReadLine());
			if (s == 'H' || s == 'h') 
			{
                
                    carga(nave, ref balas, tunel, ref enemigos);
                
            }
			while (nave.col!=-1) 
			{
				char c = ' ';
				c = LeeInput();
				
                AvanzaTunel(ref tunel);

				GeneraEnemigo(ref enemigos, tunel);

				AvanzaEnemigo(ref enemigos);
				Colisiones(ref tunel, ref nave, ref balas, ref enemigos, ref colisiones);

				if (nave.col != 1) 
				{
					AvanzaNave(c, ref nave);
					if (c == 'x')
					{
						GeneraBala(ref balas, nave);
					}
					AvanzaBalas(ref balas);

					Colisiones(ref tunel, ref nave, ref balas, ref enemigos, ref colisiones);
					if (c == 'g')
					{
						guarda(nave, balas, tunel, enemigos);
					}
                

                }
               
                Render(tunel, nave, enemigos, balas, colisiones);

				Thread.Sleep(300);

				for(int i = 0; i < colisiones.num; i++) 
				{
                   
                    EliminaEntidad(i, ref colisiones);
				}
			}
			Console.ResetColor();
			Console.SetCursorPosition(ANCHO / 2, ALTO / 2);
			Console.BackgroundColor = ConsoleColor.Green;
			Console.Write("HAS PERDIDO");
			Console.SetCursorPosition(0, 26);
			Console.ResetColor();
			
		}


		static void IniciaTunel(out Tunel tunel)
		{
			// creamos arrays
			tunel.suelo = new int[ANCHO];
			tunel.techo = new int[ANCHO];

			// rellenamos posicion 0 como semilla para generar el resto
			tunel.techo[0] = 0;
			tunel.suelo[0] = ALTO - 1;

			// dejamos 0 como la última y avanzamos hasta dar la vuelta
			tunel.ini = 1;
			for (int i = 1; i < ANCHO; i++)
			{
				AvanzaTunel(ref tunel);
			}
			// al dar la vuelta y quedará tunel.ini=0    
		}



		static void AvanzaTunel(ref Tunel tunel)
		{
			// ultima pos del tunel: anterior a ini de manera circular
			int ult = (tunel.ini + ANCHO - 1) % ANCHO;

			// valores de suelo y techo en la última posicion
			int s = tunel.suelo[ult],
				t = tunel.techo[ult]; // incremento/decremento de suelo/techo

			// generamos nueva columna a partir de esta última
			int opt = rnd.Next(5); // obtenemos un entero de [0,4]
			if (opt == 0 && s < ALTO - 1) { s++; t++; }   // tunel baja y mantiene ancho
			else if (opt == 1 && t > 0) { s--; t--; }   // sube y mantiene ancho
			else if (opt == 2 && s - t > 7) { s--; t++; } // se estrecha (como mucho a 5)
			else if (opt == 3)
			{                    // se ensancha, si puede
				if (s < ALTO - 1) s++;
				if (t > 0) t--;
			} // con 4 sigue igual

			// guardamos nueva columna del tunel generada
			tunel.suelo[tunel.ini] = s;
			tunel.techo[tunel.ini] = t;

			// avanzamos la tunel.ini: siguiente en el array circular
			tunel.ini = (tunel.ini + 1) % ANCHO;
		}

		static void renderTunel(Tunel tunel) 
		{
			int indicador = tunel.ini;		//indicador será la columna a renderizar, y coincide con el indice del array circular

			for (int c = 0; c < ANCHO; c++) //repasa cada columna
			{
				for (int f = 0; f < ALTO; f++) // cada fila
				{
				

					Console.SetCursorPosition(2 * c, f); // cambia el cursor a la siguiente columna
					if (f <= tunel.techo[indicador]) // desde 0 hasta el valor del techo pinta azul
					{
						Console.BackgroundColor = ConsoleColor.DarkRed;
						Console.WriteLine("  ");
					}
					else if (f >= tunel.techo[indicador]+1  && f <= tunel.suelo[indicador]-1) // desde el techo al suelo pinta negro
					{
						Console.BackgroundColor = ConsoleColor.DarkYellow;
						Console.WriteLine("  ");
					}
					else // desde el suelo hasta abajo azul
					{
						Console.BackgroundColor = ConsoleColor.DarkRed;
						Console.WriteLine("  ");
					}
				}
				indicador = (indicador + 1) % ANCHO;			//aumentamos indicador
			}
			Console.ResetColor();


		}
		static void AnhadeEntidad(Entidad ent , ref GrEntidades gr) 
		{
			//Pasamos por referencia ya que modificamos el número dentro de la estructura gr.
			//El máximo número de entidades posibles es la suma de las balas, enemigos y la nave
			//Anadimos la entidad a la última posición posible del array
			if (gr.num < MAX_BALAS + MAX_ENEMIGOS + 1)
			{
				gr.ent[gr.num] = ent;			
				gr.num++;
			}
		}

		static void EliminaEntidad(int i, ref GrEntidades gr) 
		{
			//Va por referencia ya que modificamos el int que tiene el struct.
			//Eliminamos la entidad, igualandola a la posición vacía y reduciendo el numero de unidades dentro del GrEntidades en uno.

			if (gr.num > 0 )				
			{
				gr.ent[i] = gr.ent[gr.num-1];			
				gr.num--;
			}
		}

		static void AvanzaNave(char ch, ref Entidad nave) 
		{
			//Casos posibles de movimiento de la nave		
			if (ch == 'l' && nave.col > 0) 
			{
				nave.col--;
			}
			else if(ch == 'r' && nave.col < ANCHO - 1) 
			{
				nave.col++;
			}
			else if (ch == 'u') 
			{
				nave.fil--;
			}
			else if (ch == 'd' ) 
			{
				nave.fil++;
			}
		}

		static void Render(Tunel tunel,Entidad nave, GrEntidades enemigos,GrEntidades balas, GrEntidades colisiones) 
		{

			//Renderizamos el tunel
			renderTunel(tunel);

			//Renderizamos la nave en caso de que no haya chocado
			if(nave.col > 0)
			{
				Console.BackgroundColor = ConsoleColor.DarkYellow;
				Console.SetCursorPosition(nave.col * 2, nave.fil);			 // dibuja la nave
				Console.Write("=>");
				Console.ResetColor();
			}
			
			//Renderizamos los enemigos 
			for (int i=0;i<enemigos.num;i++) 
			{

				//Comprobamos que la posición del enemigo sea valida, para ahorrarnos problemas
				Console.BackgroundColor = ConsoleColor.Black;
				if (enemigos.ent[i].col >0) 
				{
					Console.SetCursorPosition(enemigos.ent[i].col * 2, enemigos.ent[i].fil);
					Console.Write("<>");
					Console.ResetColor();
				}
			}

			//Renderizamos las balas
			for(int i = 0; i < balas.num; i++) 
			{
				Console.BackgroundColor = ConsoleColor.Magenta;
				if (balas.ent[i].col < ANCHO-1)
				{
					Console.SetCursorPosition(balas.ent[i].col * 2 , balas.ent[i].fil);
					Console.Write("--");
					Console.ResetColor();
				}
			}

			//Renderizamos las colisiones
			for (int i = 0; i < colisiones.num; i++) 
			{
				//Comprobamos que la colisión sea valida
				if (colisiones.ent[i].col>0 && colisiones.ent[i].fil > 0) 
				{
					Console.BackgroundColor = ConsoleColor.Magenta;
					Console.SetCursorPosition(colisiones.ent[i].col * 2, colisiones.ent[i].fil);
					Console.Write("=*");
					Console.ResetColor();
				}
			
			}

			Console.ResetColor();

			if (DEBUG) 
			{
				Console.ResetColor();
				Console.SetCursorPosition(0, 20);
				Console.WriteLine("Columna Nave: " + enemigos.ent[0].col);
				Console.WriteLine("Fila Nave: " + enemigos.ent[0].fil);

				Console.WriteLine("Fil MIN: " + tunel.techo[tunel.ini]);
				Console.WriteLine("Fil MAX: " + tunel.suelo[tunel.ini]);
			}
		}

		static void GeneraEnemigo(ref GrEntidades enemigos,Tunel tunel) 
		{

			//Generamos el enemigo en caso de que puedan haber más 
			if (enemigos.num < MAX_ENEMIGOS-1) 
			{
				//Hay una probabilidad del 25% de que aparezca
				int probabilidad = rnd.Next(4);
				if (probabilidad == 0)
				{
					//Creamos una entidad de tipo enemigo y le asignamos los parámetros pertinentes
					Entidad newEnemy = enemigos.ent[enemigos.num];											//aqui antes hacia new
					newEnemy.col = ANCHO - 1;
					newEnemy.fil = rnd.Next(tunel.techo[(tunel.ini-1+ANCHO)%ANCHO]+2, tunel.suelo[(tunel.ini - 1 + ANCHO) % ANCHO]-2);   

					//Anadimos la entidad a su grupo
					AnhadeEntidad(newEnemy, ref enemigos);
				}
			}
		}

		static void AvanzaEnemigo(ref GrEntidades enemigos)			
		{
			int i = 0;
			//Avanzamos los enemigos
			while (i < enemigos.num )
			{
				//En caso de que pueda avanzar, lo hace
				if (enemigos.ent[i].col >= 0)
				{
					enemigos.ent[i].col--;
					i = (i + 1) % ANCHO;

				}
				//Si no puede avanzar (ha llegado al límite izquierdo) lo eliminamos
				else
				{
					EliminaEntidad(i, ref enemigos);
				}
				
			}
		}


		static void GeneraBala( ref GrEntidades balas, Entidad nave) 
		{
			//Si se puede generar una bala, lo hace
			if(balas.num<MAX_BALAS && nave.col < ANCHO - 1) 
			{
				Entidad newBullet = balas.ent[balas.num];          
				newBullet.col = nave.col + 1;
				newBullet.fil = nave.fil;						
				AnhadeEntidad(newBullet, ref balas);
			}
		}


		static void AvanzaBalas(ref GrEntidades balas) 
		{

			//Avanzamos las balas de la misma manera que los enemigos
			int i = 0;
			while (i < balas.num) 
			{
				if (balas.ent[i].col < ANCHO)
				{
					balas.ent[i].col++;
					i = (i + 1) % ANCHO;

				}
				else
				{
					EliminaEntidad(i, ref balas);
				}
			}	
		}

		static void ColNaveTunel(Tunel tunel,ref Entidad nave, ref GrEntidades colisiones) 
		{
			//Indicador que lleva la cuenta de la columna a comprobar
			int indicador = (nave.col+tunel.ini) % ANCHO;


			//Comprobamos si choca ya sea con el techo o con el suelo
			if (nave.fil <= tunel.techo[indicador] || nave.fil >= tunel.suelo[indicador])             
			{
				//Creamos una entidad colisión y la añadimos
				Entidad newColision = new Entidad();
				newColision.col = nave.col;
				newColision.fil= nave.fil;
				AnhadeEntidad( newColision, ref colisiones);

				//Por último, ponemos la columna de la nave a -1, ya que hemos perdido
				nave.col = -1;
			}

			

		}

		static void ColBalasTunel(ref Tunel tunel, ref GrEntidades balas, ref GrEntidades colisiones) 
		{
			//Recorremos el array de balas dentro del mismo grupo y comprobamos si colisionan con el tunel
			for (int i = 0; i < balas.num; i++) 
			{
				//Los dos primeros condicionales comprueban si la bala choca con el tunel, cuando ambos coinciden en posición
				if (balas.ent[i].fil <= tunel.techo[(balas.ent[i].col + tunel.ini) % ANCHO])
				{

					Entidad newColision = new Entidad();
					newColision.col = balas.ent[i].col;
					newColision.fil = balas.ent[i].fil;
					EliminaEntidad(i, ref balas);

					tunel.techo[(balas.ent[i].col + tunel.ini)%ANCHO] = balas.ent[i].fil - 1;
					
					AnhadeEntidad(newColision, ref colisiones);
				}

				else if (balas.ent[i].fil >= tunel.suelo[(balas.ent[i].col + tunel.ini) % ANCHO])
				{

					Entidad newColision = new Entidad();
					newColision.col = balas.ent[i].col;
					newColision.fil = balas.ent[i].fil;

					EliminaEntidad(i, ref balas);
					tunel.suelo[(balas.ent[i].col + tunel.ini) % ANCHO] = balas.ent[i].fil + 1 ;
					AnhadeEntidad(newColision, ref colisiones);
				}
			}
		}


		static void ColNaveEnemigos(ref Entidad nave, ref GrEntidades enemigos, ref GrEntidades colisiones) 
		{
			//Recorremos el grupo de entidades de los enemigos y comprobamos si uno de ellos tiene la misma columna y fila que la nave
			for(int i = 0; i < enemigos.num; i++) 
			{
				if(nave.col == enemigos.ent[i].col && nave.fil == enemigos.ent[i].fil) 
				{
					Entidad newColision = new Entidad();
					newColision.col = nave.col;
					newColision.fil = nave.fil;
					AnhadeEntidad(newColision, ref colisiones);
					nave.col = -1;
				}
			}
		}

		static void ColBalasEnemigos(ref GrEntidades balas, ref GrEntidades enemigos, ref GrEntidades colisiones)
		{
			//Recorremos los grupos de entidades de las balas y de los enemigos, si una bala choca con un enemigo, eliminamos ambos y anadimos una colisión
			for (int i = 0; i < balas.num; i++) 
			{
				for(int j = 0; j < enemigos.num; j++) 
				{
					if (balas.ent[i].col == enemigos.ent[j].col && balas.ent[i].fil == enemigos.ent[j].fil)
					{
						EliminaEntidad(j, ref enemigos);
						EliminaEntidad(i, ref balas);

						Entidad newColision = new Entidad();
						newColision.col = balas.ent[i].col;
						newColision.fil = balas.ent[i].fil;
						AnhadeEntidad(newColision, ref colisiones);
					}
				}
			}
		}

		static void Colisiones (ref Tunel tunel,ref Entidad nave, ref GrEntidades balas, ref GrEntidades enemigos, ref GrEntidades colisiones) 
		{
			//Encapsulamos todos los metodos de colisión en uno solo
			ColNaveTunel(tunel, ref nave, ref colisiones);
			ColBalasTunel(ref tunel, ref balas, ref colisiones);
			ColNaveEnemigos(ref nave, ref enemigos, ref colisiones);
			ColBalasEnemigos(ref balas, ref enemigos, ref colisiones);
		}

		static char LeeInput()
		{
			char ch = ' ';
			if (Console.KeyAvailable)
			{
				string dir = Console.ReadKey(true).Key.ToString();
				if (dir == "A" || dir == "LeftArrow") ch = 'l';
				else if (dir == "D" || dir == "RightArrow") ch = 'r';
				else if (dir == "W" || dir == "UpArrow") ch = 'u';
				else if (dir == "S" || dir == "DownArrow") ch = 'd';
				else if (dir == "X" || dir == "Spacebar") ch = 'x'; // bala        
				else if (dir == "P") ch = 'p'; // pausa	
                else if (dir == "G") ch = 'g'; // pausa	
                else if (dir == "H") ch = 'h'; // pausa							
                else if (dir == "Q" || dir == "Escape") ch = 'q'; // salir
				while (Console.KeyAvailable) Console.ReadKey();
			}
			return ch;
		}
	}
}
