using Bomberman.Server.GameLogic;

namespace Bomberman.Server.GameLogic
{
    public class GameService
    {
        public bool isGameRunning { get; set; }
        private readonly GameState _gameState = new GameState();
        public event Action GameOver;

        public GameState GetGameState() => _gameState;

        public void StartGame(List<Player> Players)
        {
            _gameState.Playfield = new Playfield(15, 15, 0.5, Players);
            isGameRunning = true;
        }

        public void PlaceBomb(string playerId)
        {
            Player player = _gameState.Playfield.Players.FirstOrDefault(p => p.Id == playerId);
            if (player != null)
            {
                var bomb = new Bomb(playerId, (int)Math.Round(player.X), (int)Math.Round(player.Y));
                _gameState.Playfield.Bombs.Add(bomb);
            }
        }
        
        public void MovePlayer(string playerId, string direction, bool isMoving)
        {
            Player player = _gameState.Playfield.Players.FirstOrDefault(p => p.Id == playerId);
            if (player != null)
            {
                player.IsMoving = isMoving;
                player.PlayerDirection = direction;
            }
        }

        public void removePlayer(string playerId)
        {
            _gameState.Playfield.Players.RemoveAll(p => p.Id == playerId);
        }

        public void Tick()
        {
            //check if there are no players left
            if (_gameState.Playfield.Players.Count == 0)
            {
                isGameRunning = false;
                GameOver?.Invoke();
                Console.WriteLine("Stopping the game as there are no players connected");
                return;
            }
            
            //tick bombs
            var bombsToRemove = new List<Bomb>();
            foreach (var bomb in _gameState.Playfield.Bombs)
            {
                bomb.Fuse--;
                if (bomb.Fuse <= 0)
                {
                    bombsToRemove.Add(bomb);
                }
            }

            foreach (var bomb in bombsToRemove)
            {
                _gameState.Playfield.Bombs.Remove(bomb);
                ExplodeBomb(bomb);
            }

            //tick explosions
            var explosionsToRemove = new List<Explosion>();
            foreach (var explosion in _gameState.Playfield.Explosions)
            {
                explosion.Time--;
                if (explosion.Time <= 0)
                {
                    explosionsToRemove.Add(explosion);
                }
            }

            foreach (var explosion in explosionsToRemove)
            {
                _gameState.Playfield.Explosions.Remove(explosion);
            }

            //tick players
            foreach (var player in _gameState.Playfield.Players.ToList())
            {
                //if player is invincible and not in an explosion revoke invincibility
                if (player.IsInvincible && !_gameState.Playfield.Explosions.Any(e =>
                        e.X == (int)Math.Round(player.X) && e.Y == (int)Math.Round(player.Y)))
                {
                    player.IsInvincible = false;
                }

                //ckech if player is in explosion and is not invincible
                if (_gameState.Playfield.Explosions.Any(e =>
                        e.X == (int)Math.Round(player.X) && e.Y == (int)Math.Round(player.Y)) &&
                    player.IsInvincible == false)
                {
                    player.Lives--;
                    player.IsInvincible = true;
                }

                //if player has no lives left remove him
                if (player.Lives <= 0)
                {
                    _gameState.Playfield.Players.RemoveAll(p => p.Id == player.Id);
                }
                
                //move player
                if (player.IsMoving)
                {
                    switch (player.PlayerDirection)
                    {
                        case Direction.UP:
                            player.Y -= 0.1;
                            break;
                        case Direction.DOWN:
                            player.Y += 0.1;
                            break;
                        case Direction.LEFT:
                            player.X -= 0.1;
                            break;
                        case Direction.RIGHT:
                            player.X += 0.1;
                            break;
                    }
                }
                //TODO: check if player position is valid

                //TODO: check if player picked up an item
            }
        }

        private void ExplodeBomb(Bomb bomb)
        {
            // search for walls and blocks in explosion range
            // add an explosion for each valid tile

            //filter the walls to know where the explosion stops
            var walls = _gameState.Playfield.Walls.Where(w => w.X == bomb.X || w.Y == bomb.Y).ToList();
            var blocks = _gameState.Playfield.Blocks.Where(b => b.X == bomb.X || b.Y == bomb.Y).ToList();
            //step through bombs power
            List<Explosion> explosions = new List<Explosion>();
            explosions.Add(new Explosion(bomb.X, bomb.Y, bomb.OwnerId));
            //right
            for (int i = 1; i < bomb.Range; i++)
            {
                if (walls.Any(w => w.X == bomb.X + i && w.Y == bomb.Y))
                    break;
                if (blocks.Any(b => b.X == bomb.X + i && b.Y == bomb.Y))
                {
                    blocks.RemoveAll(b => b.X == bomb.X + i && b.Y == bomb.Y);
                    break;
                }

                explosions.Add(new Explosion(bomb.X + i, bomb.Y, bomb.OwnerId));
            }

            //left
            for (int i = 1; i < bomb.Range; i++)
            {
                if (walls.Any(w => w.X == bomb.X - i && w.Y == bomb.Y))
                    break;
                if (blocks.Any(b => b.X == bomb.X - i && b.Y == bomb.Y))
                {
                    blocks.RemoveAll(b => b.X == bomb.X - i && b.Y == bomb.Y);
                    break;
                }

                explosions.Add(new Explosion(bomb.X - i, bomb.Y, bomb.OwnerId));
            }

            //up
            for (int i = 1; i < bomb.Range; i++)
            {
                if (walls.Any(w => w.X == bomb.X && w.Y == bomb.Y + i))
                    break;
                if (blocks.Any(b => b.X == bomb.X && b.Y == bomb.Y + i))
                {
                    blocks.RemoveAll(b => b.X == bomb.X && b.Y == bomb.Y + i);
                    break;
                }

                explosions.Add(new Explosion(bomb.X, bomb.Y + i, bomb.OwnerId));
            }

            //down
            for (int i = 1; i < bomb.Range; i++)
            {
                if (walls.Any(w => w.X == bomb.X && w.Y == bomb.Y - i))
                    break;
                if (blocks.Any(b => b.X == bomb.X && b.Y == bomb.Y - i))
                {
                    blocks.RemoveAll(b => b.X == bomb.X && b.Y == bomb.Y - i);
                    break;
                }

                explosions.Add(new Explosion(bomb.X, bomb.Y - i, bomb.OwnerId));
            }
        }
    }
}