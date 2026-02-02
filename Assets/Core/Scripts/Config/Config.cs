using System;
using System.IO;
using UnityEngine;


namespace Game.Config
{

    [CreateAssetMenu(fileName = "GameConfig", menuName = "GameFramework/Config")]
    public class Config : ScriptableObject
    {
        public static readonly string key = "MySecretKey12345";
        public string version = "1.0.0";
        public AudioClip clickSound;
        public int defaultLives = 3;

        public static string savePath = Path.Combine(Application.persistentDataPath, "hermitfogsave.dat");

        public enum GameState { Playing, Paused, GameOver, Restart }

        [Header("Scene Mappings")]
        public static string bootScene = "Bootstrap";
        public static string loadingScene = "LoadingScene";
        public static string coreLoader = "CoreInterfaceToUser";
        public static string splashScreen = "SplashScreen";
        public static string creditsScene = "CreditsScene";
        public static string menuScene = "MainMenuScene";

        public static string optionsScene = "OptionsScene";
        public static string profileScene = "ProfileScene";
        public static string shopScene = "ShopScene";
        public static string storyLineScene = "StoryLineScene";

        // Ingame
        public static string intro = "Intro";
        public static string tutorialScene = "TutorialScene";
        public static string level1_0_Scene = "Level1_room0";
        public static string level1_1_Scene = "Level1_room1";
        public static string level1_2_Scene = "Level1_room2";
        public static string bossFightLevel_1 = "BossLevel1";
        public static string level2Scene = "Level2";

        public static float cameraWaitToplayer = 2f;
        public static float DIALOG_WAIT_TIME = 1f;
        public static bool isInCutscene = false;

        public static string SwitchStateScene(SceneState newState)
        {
            switch (newState)
            {
                case SceneState.None: return "";
                case SceneState.Boot: return bootScene;
                case SceneState.Loading: return loadingScene;
                case SceneState.CoreLoader: return coreLoader;
                case SceneState.Menu: return menuScene;
                case SceneState.Intro: return intro;
                case SceneState.Tutorial: return tutorialScene;
                case SceneState.level1_0_Scene: return level1_0_Scene;
                case SceneState.level1_1_Scene: return level1_1_Scene;
                case SceneState.level1_2_Scene: return level1_2_Scene;
                case SceneState.BossLevel_1: return bossFightLevel_1;
                case SceneState.Credits: return creditsScene;
                case SceneState.Options: return optionsScene;
                case SceneState.Profiles: return profileScene;
                case SceneState.Shops: return shopScene;
                case SceneState.StoryLines: return storyLineScene;
                case SceneState.SplashScreen: return splashScreen;
                case SceneState.Level2Scene: return level2Scene;
                default: return "None";
            }

        }

        public static SceneState SwitchStringToSceneState(string sceneString)
        {
            switch (sceneString)
            {
                case "": return SceneState.None;
                case "Bootstrap": return SceneState.Boot;
                case "LoadingScene": return SceneState.Loading;
                case "CoreInterfaceToUser": return SceneState.CoreLoader;
                case "MainMenuScene": return SceneState.Menu;
                case "Intro": return SceneState.Intro;
                case "TutorialScene": return SceneState.Tutorial;
                case "Level1_room0": return SceneState.level1_0_Scene;
                case "Level1_room1": return SceneState.level1_1_Scene;
                case "Level1_room2": return SceneState.level1_2_Scene;
                case "BossLevel1": return SceneState.BossLevel_1;
                case "CreditsScene": return SceneState.Credits;
                case "OptionsScene": return SceneState.Options;
                case "ProfileScene": return SceneState.Profiles;
                case "ShopScene": return SceneState.Shops;
                case "StoryLineScene": return SceneState.StoryLines;
                case "SplashScreen": return SceneState.SplashScreen;
                case "Level2": return SceneState.Level2Scene;
                default: return SceneState.None;
            }

        }

        // Replace the PortalState enum definition with valid C# enum member names
        public enum PortalState
        {
            State1, State2, State3, State4, State5, State6, State7, State8, State9, State10,
            State11, State12, State13, State14, State15, State16, State17, State18, State19, State20,
            State21, State22, State23, State24, State25, State26, State27, State28, State29, State30,
            State0
        }

        public enum SceneState
        {
            Boot, Loading, CoreLoader, SplashScreen, Credits, Menu,
            Options,
            Profiles,
            Shops,
            StoryLines,

            Intro,
            Tutorial,
            level1_0_Scene,
            level1_1_Scene,
            level1_2_Scene,
            BossLevel_1,
            Level2Scene,

            None,
        }
        public enum InputMode
        {
            Keyboard,
            Joystick,
            Mouse,
            Touchscreen
        }
        public enum SkillType
        {
            Attack,
            Heal,
            Buff,
            Debuff,
            Evade,
            Utility
        }

        public enum TargetType
        {
            Self,
            Enemy,
            Ally,
            Area
        }

        public enum EntityActionType
        {
            Idle,
            Walk,
            Run,
            Attack,
            Jump,
            Hit,
            Die,
            Hurt,
            Jump_Airbone,
            Jump_Airbone_Attack,
            Jump_Airbone_Land,
            Run_Attack,
            Walk_Attack
        }

        // Player states untuk dipakai motion dan animasi otomatis
        public enum PlayerState
        {
            Idle,
            Walk,
            Run,
            Jump,
            Fall,
            Attack,
            Hurt,
            Dead
        }

        public enum EntityProjectileActionType
        {
            Pre,
            Mid,
            Finish
        }

        public enum EnemyState
        {
            Idle,
            Patrol,
            Chase,
            Attack,
            Return,
            Die
        }

        public enum AxisMode
        {
            FreeXY,         // bebas
            HorizontalOnly, // hanya X
            VerticalOnly,   // hanya Y
            HorizontalAndVerticalOnly, // hanya x dan y
            FourCorner,     // diagonal saja
            EightDirection  // D-pad style
        }

        public enum DockMode
        {
            None,
            LeftHalf,
            RightHalf,
            TopHalf,
            BottomHalf
        }


        public enum MonsterType
        {
            Normal,
            Elite,
            Boss
        }

        public enum MonsterClass
        {
            Melee,
            Ranged,
            Tank,
            Assassin,
            Summoner,
            Flying,
            Hybrid,
            Ground,
        }

        public enum BehaviourMode
        {
            Passive,       // Tidak menyerang kecuali diserang
            Defensive,     // Menyerang hanya jika musuh masuk jarak dekat
            Aggressive,    // Kejar musuh terus
            Patrol,        // Jalan sana-sini
            Guard,         // Menjaga titik tetap
        }

        public enum EmoteState
        {
            AnakAyam,
            Apple,
            Bearish,
            Beer,
            Beku,
            Berapi_api,
            Berat,
            Berbintang,
            Berdarah,
            Berdenging,
            Berimajinasi,
            Berkabut,
            Berkedip,
            Berkeringat,
            Berlian,
            Bidadari,
            BintangJatuh,
            Blank,
            Bom,
            Breeze,
            Briliant,
            Buku,
            Bullish,
            Bunga,
            Buruburu,
            Cahaya,
            Cantik,
            Charming,
            Cinta,
            Cium,
            Cloud,
            Coin,
            Daging,
            Diam,
            GregetMalu,
            Gunting,
            Hadiah,
            Hantu,
            HantuTerbang,
            HartaKarun,
            Heran,
            Iblis,
            Ikan,
            Jahat,
            Jamur,
            Kertas,
            Kopi,
            Kotoran,
            Kue,
            Kusut,
            Ledakan,
            Lezat,
            Mahkota,
            Malam,
            Marah,
            MataBersinar,
            Math,
            Medali,
            Meledak,
            Melody,
            Numerik,
            Palu,
            Panik,
            PatahHati,
            Pelangi,
            Pentagram,
            Perang,
            Pusing,
            Rain,
            Ramen,
            Retak,
            Riot,
            RoamingLanguage,
            Roti,
            Sekarat,
            SerbukBintang,
            SharpTeeth,
            Singing,
            Starlight,
            Sun,
            Surat,
            TandaSeru,
            TandaSeruTanya,
            TandaTanya,
            TekanTombol,
            TepatSasaran,
            TersipuMalu,
            Thunder,
            Tidur,
            Tropi,
            Waktu
        }

        public enum ObstacleType
        {
            Static,
            Rotating,
            Moving,
            Hazard,
            Breakable
        }
        public enum ItemType { Consumable, Equipment, Quest, Misc }
        public struct GameStarted { }              // Fired when game starts
        public struct GameOver { public int FinalScore; }  // Fired when game ends
        public struct PlayerScored { public int Score; }   // Fired when player earns points
        public struct ScoreLoaded { public int SavedScore; } // Fired when save data is read
        public struct GameLoading
        {
            public float _targetProgress;
            public bool _isDoneLoad;
            public string _message;
            public bool _forceAngleReset;
            public float _angle;
            public bool _loop;
        }
        public enum LoadingAnimationMode
        {
            Tween,
            Cinematic
        }

        public enum SplashTweenType
        {
            PuddingBounce,
            Elastic,
            Punch,
            SimpleFade
        }

        public enum TweenEffect
        {
            None,
            FadeIn,
            ScalePop,
            FadeAndScale,
            SlideFromLeft,
            SlideFromRight,
            Shake
        }

        public enum CreditRole
        {
            Title,
            Summary,

            Creative,
            Production,
            Art,
            GameDesign,
            Narrative,
            Cinematics,
            Programming,
            Animation,
            Audio,
            QA,
            Marketing,
            Research,
            Localization,
            Data,
            Support,
            Additional
        }

    }

}
