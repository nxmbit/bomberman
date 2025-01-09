using Bomberman.Server.GameLogic;

namespace Bomberman.Server.GameLogic
{
    public static class OutcomeType
    {
        public const string TIME_OUT = "TIME_OUT";
        public const string ALL_ELIMINATED = "ALL_ELIMINATED";
        public const string NO_PLAYERS_CONNECTED = "NO_PLAYERS_CONNECTED";
    }

    public class GameService
    {
        public bool isGameRunning { get; set; }
        private readonly GameState _gameState = new GameState();
        public event Action<string> GameOver;

        public GameState GetGameState() => _gameState;

        public void StartGame(List<Player> Players)
        {
            _gameState.Playfield = new Playfield(15, 15, 0.5, Players);
            isGameRunning = true;
        }

        public void PlaceBomb(string playerId)
        {
            Player player = _gameState.Playfield.Players.FirstOrDefault(p => p.Id == playerId);
            int bombsPlaced = _gameState.Playfield.Bombs.Count(b => b.OwnerId == playerId);
            if (player != null && player.Lives > 0 && bombsPlaced < player.BombLimit)
            {
                var bomb = new Bomb(playerId, (int)Math.Round(player.X), (int)Math.Round(player.Y));
                _gameState.Playfield.Bombs.Add(bomb);
            }
        }

        public void MovePlayer(string playerId, string direction, bool isMoving)
        {
            Player player = _gameState.Playfield.Players.FirstOrDefault(p => p.Id == playerId);
            if (player != null && player.Lives > 0)
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
                GameOver?.Invoke(OutcomeType.NO_PLAYERS_CONNECTED);
                Console.WriteLine("Stopping the game as there are no players connected");
                return;
            }

            //tick timer
            _gameState.Playfield.Timer.Tick();
            if (_gameState.Playfield.Timer.SecondsLeft <= 0)
            {
                isGameRunning = false;
                GameOver?.Invoke(OutcomeType.TIME_OUT);
                Console.WriteLine("Stopping the game as the timer ran out");
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
            foreach (Explosion explosion in _gameState.Playfield.Explosions)
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

                //move player
                if (player.IsMoving)
                {
                    var newX = player.X;
                    var newY = player.Y;

                    switch (player.PlayerDirection)
                    {
                        case Direction.UP:
                            newY -= 0.1;
                            break;
                        case Direction.DOWN:
                            newY += 0.1;
                            break;
                        case Direction.LEFT:
                            newX -= 0.1;
                            break;
                        case Direction.RIGHT:
                            newX += 0.1;
                            break;
                    }

                    // Check for collision with walls and blocks
                    bool isCollision = _gameState.Playfield.Walls.Any(w =>
                                           newX + 1> w.X && newX < w.X + 1 &&
                                           newY + 1 > w.Y && newY < w.Y + 1) ||
                                       _gameState.Playfield.Blocks.Any(b =>
                                           newX + 1 > b.X && newX < b.X + 1 &&
                                           newY + 1 > b.Y && newY < b.Y + 1);

                    if (isCollision)
                    {
                        //snap to grid only if there is no block or wall in the direction of movement from the snapped position
                        switch (player.PlayerDirection)
                        {
                            case Direction.UP:
                                if (!_gameState.Playfield.Walls.Any(w => Math.Round(player.X) == w.X &&Math.Round(player.Y) -1 == w.Y) &&
                                    !_gameState.Playfield.Blocks.Any(b => Math.Round(player.X) == b.X && Math.Round(player.Y) - 1 == b.Y))
                                {
                                    newX = Math.Round(player.X);
                                }
                                else
                                {
                                    newX = player.X;
                                    newY = player.Y;
                                }

                                break;
                            case Direction.DOWN:
                                if (!_gameState.Playfield.Walls.Any(w => Math.Round(player.X) == w.X && Math.Round(player.Y) + 1 == w.Y) &&
                                    !_gameState.Playfield.Blocks.Any(b => Math.Round(player.X) == b.X && Math.Round(player.Y) + 1 == b.Y))
                                {
                                    newX = Math.Round(player.X);
                                }
                                else
                                {
                                    newX = player.X;
                                    newY = player.Y;
                                }

                                break;
                            case Direction.LEFT:
                                if (!_gameState.Playfield.Walls.Any(w => Math.Round(player.X) - 1 == w.X && Math.Round(player.Y) == w.Y) &&
                                    !_gameState.Playfield.Blocks.Any(b => Math.Round(player.X) - 1 == b.X && Math.Round(player.Y) == b.Y))
                                {
                                    newY = Math.Round(player.Y);
                                }
                                else
                                {
                                    newX = player.X;
                                    newY = player.Y;
                                }
                                break;
                            case Direction.RIGHT:
                                if (!_gameState.Playfield.Walls.Any(w => Math.Round(player.X) + 1 == w.X && Math.Round(player.Y) == w.Y) &&
                                    !_gameState.Playfield.Blocks.Any(b => Math.Round(player.X) + 1 == b.X && Math.Round(player.Y) == b.Y))
                                {
                                    newY = Math.Round(player.Y);
                                }
                                else
                                {
                                    newX = player.X;
                                    newY = player.Y;
                                }
                                break;
                        }
                    }

                    player.X = newX;
                    player.Y = newY;
                }

                //TODO: check if player picked up an item
            }
        }

        private void ExplodeBomb(Bomb bomb) //TODO why 2 calls to this method?
        {
            // search for walls and blocks in explosion range
            // add an explosion for each valid tile

            //filter the walls to know where the explosion stops
            var walls = _gameState.Playfield.Walls.Where(w => w.X == bomb.X || w.Y == bomb.Y).ToList();
            var blocks = _gameState.Playfield.Blocks.Where(b => b.X == bomb.X || b.Y == bomb.Y).ToList();
            List<Block> blocksToRemove = new List<Block>();
            //step through bombs power
            _gameState.Playfield.Explosions.Add(new Explosion(bomb.X, bomb.Y, bomb.OwnerId));
            //right
            for (int i = 1; i < bomb.Range; i++)
            {
                if (walls.Any(w => w.X == bomb.X + i && w.Y == bomb.Y))
                    break;
                if (blocks.Any(b => b.X == bomb.X + i && b.Y == bomb.Y))
                {
                    blocksToRemove.AddRange(blocks.FindAll(b => b.X == bomb.X + i && b.Y == bomb.Y));
                    break;
                }

                _gameState.Playfield.Explosions.Add(new Explosion(bomb.X + i, bomb.Y, bomb.OwnerId));
            }

            //left
            for (int i = 1; i < bomb.Range; i++)
            {
                if (walls.Any(w => w.X == bomb.X - i && w.Y == bomb.Y))
                    break;
                if (blocks.Any(b => b.X == bomb.X - i && b.Y == bomb.Y))
                {
                    blocksToRemove.AddRange(blocks.FindAll(b => b.X == bomb.X - i && b.Y == bomb.Y));
                    break;
                }

                _gameState.Playfield.Explosions.Add(new Explosion(bomb.X - i, bomb.Y, bomb.OwnerId));
            }

            //up
            for (int i = 1; i < bomb.Range; i++)
            {
                if (walls.Any(w => w.X == bomb.X && w.Y == bomb.Y + i))
                    break;
                if (blocks.Any(b => b.X == bomb.X && b.Y == bomb.Y + i))
                {
                    blocksToRemove.AddRange(blocks.FindAll(b => b.X == bomb.X && b.Y == bomb.Y + i));
                    break;
                }

                _gameState.Playfield.Explosions.Add(new Explosion(bomb.X, bomb.Y + i, bomb.OwnerId));
            }

            //down
            for (int i = 1; i < bomb.Range; i++)
            {
                if (walls.Any(w => w.X == bomb.X && w.Y == bomb.Y - i))
                    break;
                if (blocks.Any(b => b.X == bomb.X && b.Y == bomb.Y - i))
                {
                    blocksToRemove.AddRange(blocks.FindAll(b => b.X == bomb.X && b.Y == bomb.Y - i));
                    break;
                }

                _gameState.Playfield.Explosions.Add(new Explosion(bomb.X, bomb.Y - i, bomb.OwnerId));
            }
            //remove blocks that were destroyed
            foreach (var block in blocksToRemove)
            {
                _gameState.Playfield.Blocks.Remove(block);
            }
        }
        
    }
}