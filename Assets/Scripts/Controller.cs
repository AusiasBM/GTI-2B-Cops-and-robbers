using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Controller : MonoBehaviour
{
    //GameObjects
    public GameObject board;
    public GameObject[] cops = new GameObject[2];
    public GameObject robber;
    public Text rounds;
    public Text finalMessage;
    public Button playAgainButton;

    //Otras variables
    Tile[] tiles = new Tile[Constants.NumTiles];
    private int roundCount = 0;
    private int state;
    private int clickedTile = -1;
    private int clickedCop = 0;

    void Start()
    {
        InitTiles();
        InitAdjacencyLists();
        state = Constants.Init;
    }

    //Rellenamos el array de casillas y posicionamos las fichas
    void InitTiles()
    {
        for (int fil = 0; fil < Constants.TilesPerRow; fil++)
        {
            GameObject rowchild = board.transform.GetChild(fil).gameObject;

            for (int col = 0; col < Constants.TilesPerRow; col++)
            {
                GameObject tilechild = rowchild.transform.GetChild(col).gameObject;
                tiles[fil * Constants.TilesPerRow + col] = tilechild.GetComponent<Tile>();
            }
        }

        cops[0].GetComponent<CopMove>().currentTile = Constants.InitialCop0;
        cops[1].GetComponent<CopMove>().currentTile = Constants.InitialCop1;
        robber.GetComponent<RobberMove>().currentTile = Constants.InitialRobber;
    }

    public void InitAdjacencyLists()
    {
        //TODO: Rellenar la lista "adjacency" de cada casilla con los índices de sus casillas adyacentes
        for (int i = 0; i < tiles.Length; i++) // Recorremos la lista de tiles
        {
            if (i - 8 >= 0) // Si restamos 8 y es mayor a 0 quiere decir que tiene una casilla arriba de el
            {
                tiles[i].adjacency.Add(i - 8);
            }

            if (i + 8 < tiles.Length) // Si sumamos 8 y es menor a tiles.length, quiere decir que tiene una casilla por debajo de el
            {
                tiles[i].adjacency.Add(i + 8);
            }

            if ((i + 1) % 8 == 0 && i >= 0 && i < tiles.Length) // Si sumamos 1 a la i, i el residuo de dividirlo entre 8 es 0, sabemos que la i en que estema es el final de una fila
            {
                tiles[i].adjacency.Add(i - 1);

            }else if (i % 8 == 0 && i >= 0 && i < tiles.Length) // Si el residuo de i entre 8 es 0, sabemos que la casilla en la que estamos es la de la izquierda del todo.
            {
                tiles[i].adjacency.Add(i + 1);
            }
            else // En este else entramos si estamos por el medio de las filas
            {
                if (i + 1 < tiles.Length) 
                {
                    tiles[i].adjacency.Add(i + 1);
                }

                if (i - 1 >= 0)
                {
                    tiles[i].adjacency.Add(i - 1);
                }
            } 

        }

    }

    //Reseteamos cada casilla: color, padre, distancia y visitada
    public void ResetTiles()
    {
        foreach (Tile tile in tiles)
        {
            tile.Reset();
        }
    }

    public void ClickOnCop(int cop_id)
    {
        switch (state)
        {
            case Constants.Init:
            case Constants.CopSelected:
                clickedCop = cop_id;
                clickedTile = cops[cop_id].GetComponent<CopMove>().currentTile;
                tiles[clickedTile].current = true;

                ResetTiles();
                FindSelectableTiles(true);

                state = Constants.CopSelected;
                break;
        }
    }

    public void ClickOnTile(int t)
    {
        clickedTile = t;

        switch (state)
        {
            case Constants.CopSelected:
                //Si es una casilla roja, nos movemos
                if (tiles[clickedTile].selectable)
                {
                    cops[clickedCop].GetComponent<CopMove>().MoveToTile(tiles[clickedTile]);
                    cops[clickedCop].GetComponent<CopMove>().currentTile = tiles[clickedTile].numTile;
                    tiles[clickedTile].current = true;

                    state = Constants.TileSelected;
                }
                break;
            case Constants.TileSelected:
                state = Constants.Init;
                break;
            case Constants.RobberTurn:
                state = Constants.Init;
                break;
        }
    }

    public void FinishTurn()
    {
        switch (state)
        {
            case Constants.TileSelected:
                ResetTiles();

                state = Constants.RobberTurn;
                RobberTurn();
                break;
            case Constants.RobberTurn:
                ResetTiles();
                IncreaseRoundCount();
                if (roundCount <= Constants.MaxRounds)
                    state = Constants.Init;
                else
                    EndGame(false);
                break;
        }

    }

    public void RobberTurn()
    {
        clickedTile = robber.GetComponent<RobberMove>().currentTile;
        tiles[clickedTile].current = true;
        FindSelectableTiles(false);

        //TODO: Cambia el código de abajo para hacer lo siguiente
        // - Elegimos una casilla aleatoria entre las seleccionables que puede ir el caco

        List<int> casillaSeleccionables = new List<int>(); // Hacemos una lista de int para guardar la casillas que tiene el valor de select activo
        for (int i = 0; i < Constants.NumTiles; i++)
        {
            if (tiles[i].selectable) // Buscamos las casillas que tienen el valor de selected activo
            {
                casillaSeleccionables.Add(tiles[i].numTile); // Añadimos su posición 
            }
        }
        //Random.Range(0, robber.);

        int posicionRnd = Random.Range(0, casillaSeleccionables.Count);
        // - Movemos al caco a esa casilla
        robber.GetComponent<RobberMove>().MoveToTile(tiles[casillaSeleccionables[posicionRnd]]);
        // - Actualizamos la variable currentTile del caco a la nueva casilla
        robber.GetComponent<RobberMove>().currentTile = casillaSeleccionables[posicionRnd];

        
        
    }

    public void EndGame(bool end)
    {
        if (end)
            finalMessage.text = "You Win!";
        else
            finalMessage.text = "You Lose!";
        playAgainButton.interactable = true;
        state = Constants.End;
    }

    public void PlayAgain()
    {
        cops[0].GetComponent<CopMove>().Restart(tiles[Constants.InitialCop0]);
        cops[1].GetComponent<CopMove>().Restart(tiles[Constants.InitialCop1]);
        robber.GetComponent<RobberMove>().Restart(tiles[Constants.InitialRobber]);

        ResetTiles();

        playAgainButton.interactable = false;
        finalMessage.text = "";
        roundCount = 0;
        rounds.text = "Rounds: ";

        state = Constants.Restarting;
    }

    public void InitGame()
    {
        state = Constants.Init;

    }

    public void IncreaseRoundCount()
    {
        roundCount++;
        rounds.text = "Rounds: " + roundCount;
    }

    public void FindSelectableTiles(bool cop)
    {
        Debug.Log(clickedTile.ToString());

        int indexcurrentTile;

        if (cop == true)
            indexcurrentTile = cops[clickedCop].GetComponent<CopMove>().currentTile; // index casilla actual policia
        else
            indexcurrentTile = robber.GetComponent<RobberMove>().currentTile; // Index casilla actual ladrón

        //La ponemos rosa porque acabamos de hacer un reset
        tiles[indexcurrentTile].current = true; // Ponemos que en esa casilla está

        //Cola para el BFS
        Queue<Tile> nodes = new Queue<Tile>();

        //TODO: Implementar BFS. Los nodos seleccionables los ponemos como selectable=true

        int copNotSelected = clickedCop == 0 ? 1 : 0; // Miramos que policia no es el seleccionado.

        tiles[indexcurrentTile].visited = true; // Ponemos que está de visita en la casilla en la que está
        for (int i = 0; i < tiles[indexcurrentTile].adjacency.Count; i++) // Recorremos las casillas adjacentes de esa casilla
        {
            // casilla abajo
            if (tiles[indexcurrentTile].adjacency[i] + 8 == tiles[indexcurrentTile].numTile)// Restamos 1 a la casilla adjacente y vemos si es la casilla de la derecha
            {
                if (cops[copNotSelected].GetComponent<CopMove>().currentTile + 8 != tiles[indexcurrentTile].numTile){// Comprobamos si está el otro cop en esa casilla
                    tiles[tiles[indexcurrentTile].adjacency[i]].distance = 1;// En caso de que no esté, le ponemos distancia
                    nodes.Enqueue(tiles[tiles[indexcurrentTile].adjacency[i]]);// Y la añadimos a la cola
                }
            }
            // casilla arriba
            if (tiles[indexcurrentTile].adjacency[i] - 8 == tiles[indexcurrentTile].numTile)
            {
                if (cops[copNotSelected].GetComponent<CopMove>().currentTile - 8 != tiles[indexcurrentTile].numTile)
                {
                    tiles[tiles[indexcurrentTile].adjacency[i]].distance = 1;
                    nodes.Enqueue(tiles[tiles[indexcurrentTile].adjacency[i]]);
                }
            }

            //casilla derecha
            if (tiles[indexcurrentTile].adjacency[i] - 1 == tiles[indexcurrentTile].numTile) 
            {
                if (cops[copNotSelected].GetComponent<CopMove>().currentTile - 1 != tiles[indexcurrentTile].numTile)
                {
                    tiles[tiles[indexcurrentTile].adjacency[i]].distance = 1;
                    nodes.Enqueue(tiles[tiles[indexcurrentTile].adjacency[i]]);
                }
            }
            // casilla izquierda
            if (tiles[indexcurrentTile].adjacency[i] + 1 == tiles[indexcurrentTile].numTile)
            {
                if (cops[copNotSelected].GetComponent<CopMove>().currentTile + 1 != tiles[indexcurrentTile].numTile)
                {
                    tiles[tiles[indexcurrentTile].adjacency[i]].distance = 1;
                    nodes.Enqueue(tiles[tiles[indexcurrentTile].adjacency[i]]);
                }
            }

        }


        // Una vez tenemos en la cola las casillas adjacentes recorremos esa cola hasta que se quede vacia
        while (nodes.Count > 0)
        {
            Tile tile = nodes.Dequeue(); // obtenemos la primera casilla de la cola

            for (int j = 0; j < tile.adjacency.Count; j++) // Recorremos las casillas adjacentes de la casilla anterior
            {
                // casilla abajo
                if (tile.adjacency[j] + 8 == tile.numTile)// Vamos buscando la posición de cada casilla
                {
                    if (cops[copNotSelected].GetComponent<CopMove>().currentTile + 8 != tile.numTile)
                    {
                        tiles[tile.adjacency[j]].distance = 2;// Si no hay policia, le añadimos la distancia
                        tiles[tile.adjacency[j]].parent = tile;// Y le decimos quien es su padre
                    }
                }
                // casilla arriba
                if (tile.adjacency[j] - 8 == tile.numTile)
                {
                    if (cops[copNotSelected].GetComponent<CopMove>().currentTile - 8 != tile.numTile)
                    {
                        tiles[tile.adjacency[j]].distance = 2;
                        tiles[tile.adjacency[j]].parent = tile;
                    }
                }

                // casilla derecha
                if (tile.adjacency[j] - 1 == tile.numTile) 
                {
                    if (cops[copNotSelected].GetComponent<CopMove>().currentTile - 1 != tile.numTile)
                    { // Comprobamos si hay un policia
                        tiles[tile.adjacency[j]].distance = 2; 
                        tiles[tile.adjacency[j]].parent = tile; 
                    }
                }
                // casilla izquierda
                if (tile.adjacency[j] + 1 == tile.numTile)
                {
                    if (cops[copNotSelected].GetComponent<CopMove>().currentTile + 1 != tile.numTile) { 
                        tiles[tile.adjacency[j]].distance = 2;
                        tiles[tile.adjacency[j]].parent = tile;
                    }
                }

            }
        }
        

        
        foreach(Tile tile in tiles) // ponemos todas las casillas con distancia mayor que 1 en seleccionables
            if (tile.distance >= 1) tile.selectable = true;
        
    } 
    
     
}
   
