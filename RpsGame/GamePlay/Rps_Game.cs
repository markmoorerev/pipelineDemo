using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using RPS_GameMvc.Data;
using RPS_GameMvc.Models;


namespace RPS_GameMvc.GamePlay
{
	public class Rps_Game
	{
		
		private readonly ILogger<Rps_Game> _logger;
		private IMemoryCache _cache;
		private readonly DbContextClass _context;

		public Rps_Game(ILogger<Rps_Game> logger, IMemoryCache cache, DbContextClass context)
		{
			_logger = logger;
			_cache = cache;
			_context = context;
		}

		public Game PlayAGame()
		{
			//instantiate a Player and give a value to the Name all at once.
			Player computer = new Player() { Name = "Computer" };
			//try to find an existing player named "computer"
			if (_context.Players.Any(x => x.Name == "Computer"))
			{
				computer = _context.Players.Where(x => x.Name == "Computer").FirstOrDefault();
			}
			else
			{
				computer.Name = "Computer";
				_context.Players.Add(computer);//add the computer to List<Player> players
				_context.SaveChanges();
			}

			Player p1;//the out variable for the player
			if (!_cache.TryGetValue("loggedInPlayer", out p1))
			{
				_logger.LogInformation("There was no loggedInPlayer in the cache");
				throw new NullReferenceException("There was no loggedInPlayer in the cache");
			}

			p1 = _context.Players.Where(x => x.PlayerId == p1.PlayerId).FirstOrDefault();
			Game game = new Game();// create a game
			//_context.Players.Add(p1);
			game.Player1 = p1;//
			game.Computer = computer;//

			while (game.winner.Name == "null")
			{
				Round round = new Round();	//declare a round for this iteration
				round.player1 = p1;			// add user (p1) to this round
				round.Computer = computer;	// add computer to this round

				//get the choices for the 2 players and insert the players choices directly into the round
				round.p1Choice = Rps_GameMethods.GetRandomChoice();//this will give a random number starting at 0 to arg-1;
				round.ComputerChoice = Rps_GameMethods.GetRandomChoice();
				round.Outcome = Rps_GameMethods.GetRoundWinner(round);//check the choices to see who won.
				_context.Rounds.Add(round);
				_context.SaveChanges();
				game.rounds.Add(round);//add this round to the games' List of rounds
				int gameWinner = Rps_GameMethods.GetWinner(game);//get a number ot say is p1(1) or computer(2) won
				
				//assign the winner to the game and increment wins and losses for both
				if (gameWinner == 1)
				{
					game.winner = p1;
					p1.Wins++;//increments wins and losses.
					computer.Losses++;//increments wins and losses.
				}
				else if (gameWinner == 2)
				{
					game.winner = computer;
					p1.Losses++;//increments wins and losses.
					computer.Wins++;//increments wins and losses.
				}
			}//end of rounds loop
			_context.Games.Add(game);//save the game
			_context.SaveChanges();
			return game;
		}
		
		public Player GameLogin(string fname)
		{
			//see if the name is already in  players list
			Player p1 = _context.Players.FirstOrDefault(p => p.Name == fname);

			//OR create the player instance and save that player
			if (p1 == null)
			{
				p1 = new Player()
				{
					Name = fname
				};
				//add the player to the Db
				_context.Players.Add(p1);
				try{
					_context.SaveChanges();
				}
				catch(DbUpdateException ex)
				{
					_logger.LogError(ex.Message, "There was a SaveChanges() error in Rps_Game.GameLogin()");
				}
			}
			return p1;
		}
		
		public Player GameAddPlayer(Player player)
		{
			_cache.Set("loggedInPlayer", player);//set this player as the player logged in.
			return player;
		}

		public Player GameEditPlayer(int id)
		{
			Player player = _context.Players.Where(x => x.PlayerId == id).FirstOrDefault();
			if (player == null)
			{
				return null;
			}
			return player;
		}

		public bool GameEditPlayer(Player editedPlayer)
		{
			Player player = _context.Players.Where(x => x.PlayerId == editedPlayer.PlayerId).FirstOrDefault();
			if (player == null)
			{
				return false;
			}
			player.Name = editedPlayer.Name;
			player.Wins = editedPlayer.Wins;
			player.Losses = editedPlayer.Losses;

			_context.SaveChanges();
			return true;
		}

		public List<Player> GamePlayerList()
		{
			return _context.Players.ToList();
		}

		public bool GameDeletePlayer(int id)
		{
			//check that the player being deleted is the player logged in.
			//log him out and redirect to login page. with a message
			Player lgp = (Player)_cache.Get("loggedInPlayer");
			try{
				if (id == lgp.PlayerId)
				{
					_context.Players.Remove(_context.Players.Where(x => x.PlayerId == id).FirstOrDefault());
					_context.SaveChanges();
					return false;
				}
				_context.Players.Remove(_context.Players.Where(x => x.PlayerId == id).FirstOrDefault());
				_context.SaveChanges();
			}
			catch(Exception ex){
				_logger.LogError(ex, "\n\nThe player had played a game and could not be deleted.\n\n");
				return false;
			}

			return true;
		}

		public void GameLogout()
		{
			_cache.Remove("loggedInPlayer");
		}
	}//end of class
}//end of namespace
