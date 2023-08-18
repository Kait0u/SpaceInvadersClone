using NAudio.Vorbis;


namespace SpaceInvadersClone
{
    static class SoundBank
    {
        static string assetPath = "./Assets/Sounds/";

        static string moveSelection = assetPath + "move_selection.ogg";
        static string select = assetPath + "select.ogg";
        static string error = assetPath + "error.ogg";

        static string gameOver = assetPath + "gameover.ogg";

        static string playerFire = assetPath + "player_fire.ogg";
        static string playerDamaged = assetPath + "player_damaged.ogg";
        static string playerCrash = assetPath + "player_crash.ogg";
        static string playerExplosion = assetPath + "player_explosion.ogg";
        static string playerSacrifice = assetPath + "sacrifice.ogg";

        static string powerup = assetPath + "powerup.ogg";
        static string shieldProtect = assetPath + "shield_protect.ogg";

        static string enemyFire = assetPath + "enemy_fire.ogg";
        static string enemyMove = assetPath + "enemy_move.ogg";
        static string enemyDamaged = assetPath + "enemy_damaged.ogg";
        static string enemyExplosion = assetPath + "enemy_explosion.ogg";

        static string bossReinforcements = assetPath + "boss_reinforcements.ogg";
        static string bossDeath = assetPath + "boss_death.ogg";
        static string bossShieldBroken = assetPath + "boss_shieldbroken.ogg";

        // Intros:
        static string waveEntrance = assetPath + "wave_entrance.ogg";
        static string rainfallEntrance = assetPath + "rainfall_entrance.ogg";
        static string meteorEntrance = assetPath + "meteor_entrance.ogg";
        static string bossEntrance = assetPath + "boss_entrance.ogg";



        // Accessors

        public static VorbisWaveReader MoveSelection => new VorbisWaveReader(moveSelection);
        public static VorbisWaveReader Select => new VorbisWaveReader(select);
        public static VorbisWaveReader Error => new VorbisWaveReader(error);

        public static VorbisWaveReader GameOver => new VorbisWaveReader(gameOver);

        public static VorbisWaveReader PlayerFire => new VorbisWaveReader(playerFire);
        public static VorbisWaveReader PlayerDamaged => new VorbisWaveReader(playerDamaged);
        public static VorbisWaveReader PlayerCrash => new VorbisWaveReader(playerCrash);
        public static VorbisWaveReader PlayerExplosion => new VorbisWaveReader(playerExplosion);
        public static VorbisWaveReader PlayerSacrifice => new VorbisWaveReader(playerSacrifice);

        public static VorbisWaveReader Powerup => new VorbisWaveReader(powerup);
        public static VorbisWaveReader ShieldProtect => new VorbisWaveReader(shieldProtect);

        public static VorbisWaveReader EnemyFire => new VorbisWaveReader(enemyFire);
        public static VorbisWaveReader EnemyMove => new VorbisWaveReader(enemyMove);
        public static VorbisWaveReader EnemyDamaged => new VorbisWaveReader(enemyDamaged);
        public static VorbisWaveReader EnemyExplosion => new VorbisWaveReader(enemyExplosion);

        public static VorbisWaveReader BossReinforcements => new VorbisWaveReader(bossReinforcements);
        public static VorbisWaveReader BossDeath => new VorbisWaveReader(bossDeath);
        public static VorbisWaveReader BossShieldBroken => new VorbisWaveReader(bossShieldBroken);

        // Intros
        public static VorbisWaveReader WaveEntrance => new VorbisWaveReader(waveEntrance);
        public static VorbisWaveReader RainfallEntrance => new VorbisWaveReader(rainfallEntrance);
        public static VorbisWaveReader MeteorEntrance => new VorbisWaveReader(meteorEntrance);
        public static VorbisWaveReader BossEntrance => new VorbisWaveReader(bossEntrance);
    }
}
