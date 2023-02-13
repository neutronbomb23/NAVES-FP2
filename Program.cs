// using ...
using System;
using System.Threading;
using System.Diagnostics;


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

		
		static void Main() 
		{
			Console.CursorVisible = false;
			Tunel tunel = new Tunel();
			Entidad nave = new Entidad();


			GrEntidades enemigos= new GrEntidades();
			enemigos.ent = new Entidad[MAX_ENEMIGOS];
			enemigos.num= 0;

			

			nave.col = ANCHO / 2;
			nave.fil = ALTO / 2;
			tunel.ini = 5;
			IniciaTunel(out tunel);
			renderTunel(tunel);
			GeneraEnemigo(ref enemigos, tunel);
			while (true) 
			{
				char c = ' ';
				c = LeeInput();
				AvanzaTunel(ref tunel);

				GeneraEnemigo(ref enemigos, tunel);

				AvanzaEnemigo(ref enemigos);

				AvanzaNave(c, ref nave);
				
				Render(tunel, nave,enemigos);
			
				Thread.Sleep(100);


				Console.SetCursorPosition(0, 26);
				Console.WriteLine("Columna Nave: " + enemigos.num);
			}
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
			int indicador = tunel.ini;
			for (int c = 0; c < ANCHO; c++) //repasa cada columna
			{
				for (int f = 0; f < ALTO; f++) // cada fila
				{
					//Console.SetCursorPosition(15,0); // cambia el cursor a la siguiente columna

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
				indicador = (indicador + 1) % ANCHO;
			}
			Console.ResetColor();


		}
		static void AnhadeEntidad(Entidad ent , ref GrEntidades gr) 
		{
			//Pasamos por referencia ya que modificamos el número dentro de la estructura gr.
			if (gr.num < MAX_BALAS + MAX_ENEMIGOS + 1) 
			{
				gr.ent[gr.num] = ent;			//ERROR
				gr.num++;
			}
			else
			{	
				Console.SetCursorPosition(0, 20);
				Console.WriteLine("No se pueden añadir más entidades");
			}
		}

		static void EliminaEntidad(int i, ref GrEntidades gr) 
		{
			//Va por referencia ya que modificamos el int que tiene el struct
			if (gr.num > 0 )				//lo de < que 9 es raro
			{
				gr.ent[i] = gr.ent[gr.num];					//intetamos acceder a gr.num == 9; por eso da error en el array.
				gr.num--;
			}
		}



		
		static void AvanzaNave(char ch, ref Entidad nave) 
		{
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

			//Límites de la nave para que no se salga de pantalla
			//if(nave.col>ANCHO-1)nave.col= ANCHO-1;
			//if(nave.col<0)nave.col=0;
		}

		static void Render(Tunel tunel,Entidad nave, GrEntidades enemigos) 
		{
			renderTunel(tunel);

			
			if(nave.col > 0)
			{
				Console.BackgroundColor = ConsoleColor.DarkYellow;
				Console.SetCursorPosition(nave.col * 2, nave.fil); // dibuja la nave
				Console.Write("=>");
				Console.ResetColor();

			}
			for (int i=0;i<enemigos.num;i++) 
			{
				Console.BackgroundColor = ConsoleColor.Black;
				if (enemigos.ent[i].col >0) 
				{
					Console.SetCursorPosition(enemigos.ent[i].col * 2, enemigos.ent[i].fil); // dibuja la nave
					Console.Write("<>");
					Console.ResetColor();
				}
			
			}




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
			if (enemigos.num < MAX_ENEMIGOS-1) 
			{
				int probabilidad = rnd.Next(4);
				if (probabilidad == 0)
				{
					Entidad newEnemy = new Entidad();
					newEnemy.col = ANCHO-1 ;
					newEnemy.fil = rnd.Next(tunel.techo[tunel.ini]+1, tunel.suelo[tunel.ini]-1);    //Fallos

					Console.ResetColor();
					Console.SetCursorPosition(0, 25);
					Console.WriteLine("Columna NaveS: " + newEnemy.fil);

					AnhadeEntidad(newEnemy, ref enemigos);
				}
			}
		}
		static void AvanzaEnemigo (ref GrEntidades enemigos)				//Bien
		{
			for(int i=0;i<enemigos.num;i++) 
			{
				if (enemigos.ent[i].col >= 0)
				{
					enemigos.ent[i].col--;
				}
				else 
				{
					EliminaEntidad(i, ref enemigos);
				}
			}

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
				else if (dir == "Q" || dir == "Escape") ch = 'q'; // salir
				while (Console.KeyAvailable) Console.ReadKey();
			}
			return ch;
		}
	}
}
