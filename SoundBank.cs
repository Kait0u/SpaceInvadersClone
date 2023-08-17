using SFML.Audio;

namespace SpaceInvadersClone
{
    static class SoundBank
    {
        static string assetPath = "./Assets/Sounds/";
        
        static SoundBuffer moveSelection = new SoundBuffer(assetPath + "move_selection.ogg");
        static SoundBuffer select = new SoundBuffer(assetPath + "select.ogg");
        static SoundBuffer error = new SoundBuffer(assetPath + "error.ogg");

        static SoundBuffer gameOver = new SoundBuffer(assetPath + "gameover.ogg");

        static SoundBuffer playerFire = new SoundBuffer(assetPath + "player_fire.ogg");
        static SoundBuffer playerDamaged = new SoundBuffer(assetPath + "player_damaged.ogg");
        static SoundBuffer playerCrash = new SoundBuffer(assetPath + "player_crash.ogg");
        static SoundBuffer playerExplosion = new SoundBuffer(assetPath + "player_explosion.ogg");
        static SoundBuffer playerSacrifice = new SoundBuffer(assetPath + "sacrifice.ogg");

        static SoundBuffer powerup = new SoundBuffer(assetPath + "powerup.ogg");
        static SoundBuffer shieldProtect = new SoundBuffer(assetPath + "shield_protect.ogg");

        static SoundBuffer enemyFire = new SoundBuffer(assetPath + "enemy_fire.ogg");
        static SoundBuffer enemyMove = new SoundBuffer(assetPath + "enemy_move.ogg");
        static SoundBuffer enemyDamaged = new SoundBuffer(assetPath + "enemy_damaged.ogg");
        static SoundBuffer enemyExplosion = new SoundBuffer(assetPath + "enemy_explosion.ogg");

        static SoundBuffer bossReinforcements = new SoundBuffer(assetPath + "boss_reinforcements.ogg");
        static SoundBuffer bossDeath = new SoundBuffer(assetPath + "boss_death.ogg");

        // Intros:
        static SoundBuffer waveEntrance = new SoundBuffer(assetPath + "wave_entrance.ogg");
        static SoundBuffer rainfallEntrance = new SoundBuffer(assetPath + "rainfall_entrance.ogg");
        static SoundBuffer meteorEntrance = new SoundBuffer(assetPath + "meteor_entrance.ogg");
        static SoundBuffer bossEntrance = new SoundBuffer(assetPath + "boss_entrance.ogg");



        // Accessors

        public static SoundBuffer MoveSelection { get { return moveSelection; } }
        public static SoundBuffer Select { get { return select; } }
        public static SoundBuffer Error { get { return error; } }

        public static SoundBuffer GameOver { get { return gameOver; } }

        public static SoundBuffer PlayerFire { get {  return playerFire; } }
        public static SoundBuffer PlayerDamaged { get { return playerDamaged; }}
        public static SoundBuffer PlayerCrash { get {  return playerCrash; } }
        public static SoundBuffer PlayerExplosion { get { return playerExplosion; } }
        public static SoundBuffer PlayerSacrifice { get { return playerSacrifice; } }

        public static SoundBuffer Powerup { get { return powerup; } }
        public static SoundBuffer ShieldProtect { get { return shieldProtect; } }

        public static SoundBuffer EnemyFire { get { return enemyFire; } }
        public static SoundBuffer EnemyMove { get {  return enemyMove; } }
        public static SoundBuffer EnemyDamaged { get { return enemyDamaged; } }
        public static SoundBuffer EnemyExplosion { get { return enemyExplosion; } }

        public static SoundBuffer BossReinforcements => bossReinforcements;
        public static SoundBuffer BossDeath => bossDeath;

        // Intros
        public static SoundBuffer WaveEntrance => waveEntrance;
        public static SoundBuffer RainfallEntrance => rainfallEntrance;
        public static SoundBuffer MeteorEntrance => meteorEntrance;
        public static SoundBuffer BossEntrance => bossEntrance;
    }
}
