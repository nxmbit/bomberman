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
        private readonly ScoreboardService _scoreboardService;
        public event Action<string, string?, bool> GameOver;

        private const int POINTS_BLOCK_DESTROYED = 10;
        private const int POINTS_ITEM_PICKED_UP = 25;
        private const int POINTS_PLAYER_KILLED = 50;
        private const int POINTS_PLAYER_ELIMINATED = 100;

        // dictionaries for holding statistics for each player
        private Dictionary<string, int> _playerKills;
        private Dictionary<string, int> _playerDeaths;
        private Dictionary<string, int> _playerPickedPowerups;
        private Dictionary<string, int> _playerEliminations;

        public GameService(ScoreboardService scoreboardService)
        {
            _scoreboardService = scoreboardService;
        }

        public GameState GetGameState() => _gameState;

        public void StartGame(List<Player> Players, GameParameters gameParameters)
        {
            _gameState.Playfield = new Playfield(gameParameters, Players);

            _playerKills = new Dictionary<string, int>();
            _playerDeaths = new Dictionary<string, int>();
            _playerPickedPowerups = new Dictionary<string, int>();
            _playerEliminations = new Dictionary<string, int>();

            foreach (var player in Players)
            {
                _playerKills.Add(player.Id, 0);
                _playerDeaths.Add(player.Id, 0);
                _playerPickedPowerups.Add(player.Id, 0);
                _playerEliminations.Add(player.Id, 0);
            }

            isGameRunning = true;
        }

        public void PlaceBomb(string playerId)
        {
            Player player = _gameState.Playfield.Players.FirstOrDefault(p => p.Id == playerId);
            int bombsPlaced = _gameState.Playfield.Bombs.Count(b => b.OwnerId == playerId);
            if (player != null && player.Lives > 0 && bombsPlaced < player.BombLimit &&
                !_gameState.Playfield.Bombs.Any(b =>
                    b.X == (int)Math.Round(player.X) && b.Y == (int)Math.Round(player.Y)))
            {
                var bomb = new Bomb(playerId, (int)Math.Round(player.X), (int)Math.Round(player.Y), player.BombPower);
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
            else if (player != null && player.Lives <= 0)
            {
                player.IsMoving = false;
            }
        }

        public void removePlayer(string playerId)
        {
            _gameState.Playfield.Players.RemoveAll(p => p.Id == playerId);
        }

        private (bool isDraw, string? winnerName, string? winnerId) GetEndGameInfo()
        {
            var winners = _gameState.Playfield.Players
                .Where(p => p.Lives > 0).ToList();

            bool isDraw = winners.Count > 1;
            string? winnerName = isDraw ? null : winners.FirstOrDefault()?.Name;
            string? winnerId = isDraw ? null : winners.FirstOrDefault()?.Id;

            return (isDraw, winnerName, winnerId);
        }

        private void UpdateScoreboardEndGame(bool isDraw, string? winnerId)
        {
            foreach (var player in _gameState.Playfield.Players)
            {
                var user = player.GetUser();
                if (user != null)
                {
                    if (isDraw)
                    {
                        _scoreboardService.AddOrUpdateScoreboardEntry(user, player.Score, false, _playerKills[player.Id],
                            _playerEliminations[player.Id], _playerDeaths[player.Id], _playerPickedPowerups[player.Id]);
                    }
                    else if (player.Id == winnerId)
                    {
                        _scoreboardService.AddOrUpdateScoreboardEntry(user, player.Score, true, _playerKills[player.Id],
                            _playerEliminations[player.Id], _playerDeaths[player.Id], _playerPickedPowerups[player.Id]);
                    }
                    else
                    {
                        _scoreboardService.AddOrUpdateScoreboardEntry(user, player.Score, false, _playerKills[player.Id],
                            _playerEliminations[player.Id], _playerDeaths[player.Id], _playerPickedPowerups[player.Id]);
                    }
                }
            }

        }

        public void Tick()
        {
            if (!isGameRunning)
            {
                return;
            }

            //check if there are no players left
            if (_gameState.Playfield.Players.Count == 0)
            {
                isGameRunning = false;
                var (isDraw, winnerName, winnerId) = GetEndGameInfo();
                GameOver?.Invoke(OutcomeType.NO_PLAYERS_CONNECTED, winnerName, isDraw);
                Console.WriteLine("Stopping the game as there are no players connected");
                return;
            }

            //check if there is only one player left
            if (_gameState.Playfield.Players.FindAll((p)=>p.Lives>0).Count == 1)
            {
                isGameRunning = false;
                var (isDraw, winnerName, winnerId) = GetEndGameInfo();
                UpdateScoreboardEndGame(isDraw, winnerId);
                GameOver?.Invoke(OutcomeType.ALL_ELIMINATED, winnerName, isDraw);
                Console.WriteLine("Stopping the game as there is only one player left");
                return;
            }

            //tick timer
            _gameState.Playfield.Timer.Tick();
            if (_gameState.Playfield.Timer.SecondsLeft <= 0)
            {
                isGameRunning = false;
                var (isDraw, winnerName, winnerId) = GetEndGameInfo();
                UpdateScoreboardEndGame(isDraw, winnerId);
                GameOver?.Invoke(OutcomeType.TIME_OUT, winnerName, isDraw);
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
                // tick invincibility
                if (player.IsInvincible)
                {
                    player.InvincibilityTicks--;
                    if (player.InvincibilityTicks <= 0)
                    {
                        player.IsInvincible = false;
                        player.InvincibilityTicks = 0;
                    }
                }

                // if player is in an explosion and not invincible, remove a life and make player invincible
                var explosion = _gameState.Playfield.Explosions.FirstOrDefault(e =>
                    e.X == (int)Math.Round(player.X) && e.Y == (int)Math.Round(player.Y));

                if (explosion != null && !player.IsInvincible)
                {
                    player.Lives--;
                    player.IsInvincible = true;
                    player.InvincibilityTicks = GlobalSettings.INVINCIBILITY_TIME * GlobalSettings.TICK_RATE;

                    if (player.Lives <= 0)
                    {
                        player.X = 0;
                        player.Y = 0;
                        // TODO: how many times player was eliminated - store in db
                    }
                    else
                    {
                        _playerDeaths[player.Id]++;
                    }

                    // add points to the player who caused the explosion
                    var owner = _gameState.Playfield.Players.FirstOrDefault(p =>
                        p.Id == explosion.OwnerId && player.Id != explosion.OwnerId);
                    if (owner != null)
                    {
                        if (player.Lives <= 0)
                        {
                            owner.Score += POINTS_PLAYER_ELIMINATED;
                            _playerEliminations[owner.Id]++;
                        }
                        else
                        {
                            owner.Score += POINTS_PLAYER_KILLED;
                            _playerKills[owner.Id]++;
                        }
                    }
                }

                //move player
                if (player.IsMoving)
                {
                    var newX = player.X;
                    var newY = player.Y;

                    switch (player.PlayerDirection)
                    {
                        case Direction.UP:
                            newY -= 0.1 + 0.04 * player.Speed;
                            break;
                        case Direction.DOWN:
                            newY += 0.1 + 0.04 * player.Speed;
                            break;
                        case Direction.LEFT:
                            newX -= 0.1 + 0.04 * player.Speed;
                            break;
                        case Direction.RIGHT:
                            newX += 0.1 + 0.04 * player.Speed;
                            break;
                    }

                    // Check for collision with walls and blocks
                    bool isCollision = _gameState.Playfield.Walls.Any(w =>
                                           newX + 1 > w.X && newX < w.X + 1 &&
                                           newY + 1 > w.Y && newY < w.Y + 1) ||
                                       _gameState.Playfield.Blocks.Any(b =>
                                           newX + 1 > b.X && newX < b.X + 1 &&
                                           newY + 1 > b.Y && newY < b.Y + 1) ||
                                       _gameState.Playfield.Bombs.Any(b =>
                                           newX + 1 > b.X && newX < b.X + 1 &&
                                           newY + 1 > b.Y && newY < b.Y + 1);

                    if (isCollision)
                    {
                        //snap to grid only if there is no block or wall in the direction of movement from the snapped position
                        switch (player.PlayerDirection)
                        {
                            case Direction.UP:
                                if (!_gameState.Playfield.Walls.Any(w =>
                                        Math.Round(player.X) == w.X && Math.Round(player.Y) - 1 == w.Y) &&
                                    !_gameState.Playfield.Blocks.Any(b =>
                                        Math.Round(player.X) == b.X && Math.Round(player.Y) - 1 == b.Y) &&
                                    !_gameState.Playfield.Bombs.Any(b =>
                                        Math.Round(player.X) == b.X && Math.Round(player.Y) - 1 == b.Y))
                                {
                                    newX = Math.Round(player.X);
                                }
                                else
                                {
                                    newX = player.X;
                                    newY = Math.Round(player.Y);
                                }

                                break;
                            case Direction.DOWN:
                                if (!_gameState.Playfield.Walls.Any(w =>
                                        Math.Round(player.X) == w.X && Math.Round(player.Y) + 1 == w.Y) &&
                                    !_gameState.Playfield.Blocks.Any(b =>
                                        Math.Round(player.X) == b.X && Math.Round(player.Y) + 1 == b.Y) &&
                                    !_gameState.Playfield.Bombs.Any(b =>
                                        Math.Round(player.X) == b.X && Math.Round(player.Y) + 1 == b.Y))
                                {
                                    newX = Math.Round(player.X);
                                }
                                else
                                {
                                    newX = player.X;
                                    newY = Math.Round(player.Y);
                                }

                                break;
                            case Direction.LEFT:
                                if (!_gameState.Playfield.Walls.Any(w =>
                                        Math.Round(player.X) - 1 == w.X && Math.Round(player.Y) == w.Y) &&
                                    !_gameState.Playfield.Blocks.Any(b =>
                                        Math.Round(player.X) - 1 == b.X && Math.Round(player.Y) == b.Y) &&
                                    !_gameState.Playfield.Bombs.Any(b =>
                                        Math.Round(player.X) - 1 == b.X && Math.Round(player.Y) == b.Y))
                                {
                                    newY = Math.Round(player.Y);
                                }
                                else
                                {
                                    newX = Math.Round(player.X);
                                    newY = player.Y;
                                }

                                break;
                            case Direction.RIGHT:
                                if (!_gameState.Playfield.Walls.Any(w =>
                                        Math.Round(player.X) + 1 == w.X && Math.Round(player.Y) == w.Y) &&
                                    !_gameState.Playfield.Blocks.Any(b =>
                                        Math.Round(player.X) + 1 == b.X && Math.Round(player.Y) == b.Y) &&
                                    !_gameState.Playfield.Bombs.Any(b =>
                                        Math.Round(player.X) + 1 == b.X && Math.Round(player.Y) == b.Y))
                                {
                                    newY = Math.Round(player.Y);
                                }
                                else
                                {
                                    newX = Math.Round(player.X);
                                    newY = player.Y;
                                }

                                break;
                        }
                    }

                    player.X = newX;
                    player.Y = newY;
                }

                // Check if player picked up an item
                var item = _gameState.Playfield.Items.FirstOrDefault(i =>
                    Math.Round(player.X) == i.X && Math.Round(player.Y) == i.Y);

                if (item != null)
                {
                    // Apply item effect to player
                    switch (item.Type)
                    {
                        case ItemType.BOMB_UP:
                            player.BombLimit++;
                            break;
                        case ItemType.EXPLOSION_RANGE_UP:
                            player.BombPower++;
                            break;
                        case ItemType.SPEED_UP:
                            player.Speed++;
                            break;
                        case ItemType.LIFE_UP:
                            player.Lives++;
                            break;
                    }

                    // Remove item from playfield
                    _gameState.Playfield.Items.Remove(item);

                    // Add points to player for picking up the item
                    player.Score += POINTS_ITEM_PICKED_UP;
                    _playerPickedPowerups[player.Id]++;
                }
            }
        }

        private void ExplodeBomb(Bomb bomb) //TODO why 2 calls to this method?
        {
            Console.WriteLine("Exploding bomb at " + bomb.X + ", " + bomb.Y);
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
                    _gameState.Playfield.Explosions.Add(new Explosion(bomb.X + i, bomb.Y, bomb.OwnerId));
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
                    _gameState.Playfield.Explosions.Add(new Explosion(bomb.X - i, bomb.Y, bomb.OwnerId));
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
                    _gameState.Playfield.Explosions.Add(new Explosion(bomb.X, bomb.Y + i, bomb.OwnerId));
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
                    _gameState.Playfield.Explosions.Add(new Explosion(bomb.X, bomb.Y - i, bomb.OwnerId));
                    break;
                }

                _gameState.Playfield.Explosions.Add(new Explosion(bomb.X, bomb.Y - i, bomb.OwnerId));
            }

            //remove blocks that were destroyed and give points to the player
            foreach (var block in blocksToRemove)
            {
                _gameState.Playfield.Players.FirstOrDefault(p => p.Id == bomb.OwnerId).Score += POINTS_BLOCK_DESTROYED;
                _gameState.Playfield.Blocks.Remove(block);
            }
        }
    }
}