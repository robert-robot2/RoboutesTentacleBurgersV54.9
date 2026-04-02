namespace RoboutesTentacleBurgers.iStyles
{
    public interface IBloodStyles
    {

        public static MarkupString GetCss()
        {
            string css = @"




            /* Paste contents of iFonts.css here */





@media (min-width: 240px) {

    /* Core Shell */
    .bloodwyrm-shell {
        width: 1024px;
        height: 768px;
        position: relative;
        overflow: hidden;
        outline: none;
        margin: 0 auto;
        background-color: rgba(0,0,0,1);
        z-index: 1;
    }

   
    

    /* Background */
    .bloodwyrm-bg {
        position: absolute;
        width: 2048px;
        height: 2048px;
        object-fit: cover;
        /* background-color: rgba(0,255,0,.2);        */
        z-index: 2;
    }

    /* Scrollable Map */
    .bloodwyrm-map {
        position: absolute;
        width: 2048px;
        height: 2048px;
        /* background-color: rgba(255,0,0,.2);        */
        z-index: 3;
    }

    /* HUD Overlay */
    .bloodwyrm-title {
        position: absolute;
        top: 10%;
        left: 50%;
        transform: translateX(-50%);
        font-family: 'Diablo', sans-serif;
        font-size: 48px;
        color: red;
        text-shadow: 0 0 8px #f00, 0 0 16px #900, 0 0 32px #600;
        z-index: 9500;
        pointer-events: none;
    }
    /* HUD Overlay */
    .bloodwyrm-fire {
        position: absolute;
        top: 9%;
        left: 27%;
        font-family: 'Diablo', sans-serif;
        font-size: 48px;
        color: red;
        text-shadow: 0 0 8px #f00, 0 0 16px #900, 0 0 32px #600;
        z-index: 9500;
        pointer-events: none;
    }
    /* HUD Overlay */
    .bloodwyrm-subtitle {
        position: absolute;
        top: 10%;
        left: 50%;
        transform: translateX(-50%);
        font-family: 'Diablo', sans-serif;
        font-size: 36px;
        color: red;
        text-shadow: 0 0 8px #f00, 0 0 16px #900, 0 0 32px #600;
        z-index: 9500;
    }
    .bloodwyrm-subtitletext {
        position: absolute;
        top: 20%;
        left: 50%;
        transform: translateX(-50%);
        font-family: 'Diablo', sans-serif;
        font-size: 24px;
        color: red;
        text-shadow: 0 0 8px #f00, 0 0 16px #900, 0 0 32px #600;
        z-index: 9500;
    }
    .bloodwyrm-subtitletext2 {
        position: absolute;
        top: 20%;
        left: 50%;
        transform: translateX(-50%);
        font-family: 'Diablo', sans-serif;
        font-size: 20px;
        color: red;
        text-shadow: 0 0 8px #f00, 0 0 16px #900, 0 0 32px #600;
        z-index: 9000;
    }
    .bloodwyrm-subtitletoggle {
        position: absolute;
        top: 60%;
        left: 40%;
        transform: translateX(-50%);
        font-family: 'Diablo', sans-serif;
        font-size: 24px;
        color: red;
        text-shadow: 0 0 8px #f00, 0 0 16px #900, 0 0 32px #600;
        z-index: 9000;
    }
    .bloodwyrm-subtitletoggle2 {
        position: absolute;
        top: 65%;
        left: 50%;
        transform: translateX(-50%);
        font-family: 'Diablo', sans-serif;
        font-size: 24px;
        color: red;
        text-shadow: 0 0 8px #f00, 0 0 16px #900, 0 0 32px #600;
        z-index: 9500;
    }
    /* HUD Overlay */
    .bloodwyrm-hud {
        position: absolute;
        top: 30%;
        left: 50%;
        transform: translateX(-50%);
        font-family: 'Diablo', sans-serif;
        font-size: 36px;
        color: red;
        text-shadow: 0 0 8px #f00, 0 0 16px #900, 0 0 32px #600;
        z-index: 9500;
    }
    /* Inv Theme Display */

    .glow-buttonInv {
        position: absolute;
        top: 45%;
        left: 45%;
        width: 768px;
        height: 650px;
        font-family: 'Diablo';
        transform: translate(-50%, -50%);
        background-image: url('../iAssets/GothicFrame05.png');
        font-size: 1.2rem;
        font-weight: bold;
        border: none;
        border-radius: 8px;
        cursor: pointer;
        transition: all 0.3s ease-in-out;
        z-index: 9501;
    }

    /* Char Hp Theme Display */


    .char-hp-frame {
        position: absolute;
        top: 0;
        left: 0;
        width: 100%;
        height: 100%;
        background-image: url('/iAssets/GothicFrame03.png');
        background-size: cover;
        z-index: 9500;
    }
    .char-hp-box {
        position: absolute;
        left: 20px;
        top: 600px;
        width: 200px;
        height: 30px;
        overflow: hidden;
        display: flex;
        flex-direction: row;
        align-items: center;
        justify-content: flex-start;
        z-index: 9500;
    }

    .char-hp-text {
        position: relative;
        z-index: 2;
        font-size: 20px;
        text-align: center;
        margin-left: 10px;
        writing-mode: horizontal-tb;
        padding-bottom: 1.5rem;
        padding-left: 4rem;
        z-index: 9501;
        font-family: 'Diablo', sans-serif;
    }

    .char-hp-bar {
        position: absolute;
        top: 0;
        left: 0;
        height: 100%;
        z-index: 1;
        transition: width 0.2s;
        z-index: 9500;
    }

    /* Char Energy Theme Display */



    .char-energy-frame {
        position: absolute;
        top: 0;
        left: 0;
        width: 100%;
        height: 100%;
        background-image: url('/iAssets/GothicFrame03.png');
        background-size: cover;
        z-index: 9500;
    }

    .char-energy-box {
        position: absolute;
        left: 20px;
        top: 650px;
        width: 200px;
        height: 30px;
        overflow: hidden;
        display: flex;
        flex-direction: row;
        align-items: center;
        justify-content: flex-start;
        z-index: 9500;
    }

    .char-energy-text {
        position: relative;
        z-index: 2;
        font-size: 20px;
        text-align: center;
        margin-left: 10px;
        writing-mode: horizontal-tb;
        padding-bottom: 1.5rem;
        padding-left: 4rem;
        z-index: 9501;
        font-family: 'Diablo', sans-serif;
    }

    .char-energy-bar {
        position: absolute;
        top: 0;
        left: 0;
        height: 100%;
        z-index: 1;
        transition: width 0.2s;
        z-index: 9500;
    }



    .char-hunger-frame {
        position: absolute;
        top: 0;
        left: 0;
        width: 100%;
        height: 100%;
        background-image: url('/iAssets/GothicFrame03.png');
        background-size: cover;
        z-index: 9500;
    }

    .char-hunger-box {
        position: absolute;
        left: 250px;
        top: 360px;
        width: 200px;
        height: 30px;
        overflow: hidden;
        display: flex;
        flex-direction: row;
        align-items: center;
        justify-content: flex-start;
        z-index: 9500;
    }

    .char-hunger-text {
        position: relative;
        z-index: 2;
        font-size: 20px;
        text-align: center;
        margin-left: 10px;
        writing-mode: horizontal-tb;
        padding-bottom: 1.5rem;
        padding-left: 4rem;
        z-index: 9501;
    }

    .char-hunger-bar {
        position: absolute;
        top: 0;
        left: 0;
        height: 100%;
        z-index: 1;
        transition: width 0.2s;
        z-index: 9500;
    }




    .glow-buttonmap {
        position: absolute;
        font-family: 'Diablo';
        background: linear-gradient(145deg, #8b0000, #000000);
        color: darkred;
        font-size: 1.2rem;
        font-weight: bold;
        border: none;
        border-radius: 8px;
        box-shadow: 0 0 10px #ff0000, 0 0 20px #ff0000, 0 0 30px #ff0000;
        cursor: pointer;
        transition: all 0.3s ease-in-out;
        z-index: 9500;
    }

        .glow-buttonMap:hover {
            box-shadow: 0 0 15px #ff3333, 0 0 25px #ff3333, 0 0 40px #ff3333;
          
        }


    .glow-button {
        position: absolute;
        top: 50%;
        left: 50%;
        width: 200px;
        height: 60px;
        font-family: 'Diablo';
        transform: translate(-50%, -50%);
        background: linear-gradient(145deg, #8b0000, #000000);
        color: darkred;
        font-size: 1.2rem;
        font-weight: bold;
        border: none;
        border-radius: 8px;
        box-shadow: 0 0 10px #ff0000, 0 0 20px #ff0000, 0 0 30px #ff0000;
        cursor: pointer;
        transition: all 0.3s ease-in-out;
        z-index: 9500;
    }

        .glow-button:hover {
            box-shadow: 0 0 15px #ff3333, 0 0 25px #ff3333, 0 0 40px #ff3333;
            transform: translate(-50%, -50%) scale(1.05);
        }

    .glow-button2 {
        position: absolute;
        top: 60%;
        left: 50%;
        width: 200px;
        height: 60px;
        font-family: 'Diablo';
        transform: translate(-50%, -50%);
        background: linear-gradient(145deg, #8b0000, #000000);
        color: darkred;
        font-size: 1.2rem;
        font-weight: bold;
        border: none;
        border-radius: 8px;
        box-shadow: 0 0 10px #ff0000, 0 0 20px #ff0000, 0 0 30px #ff0000;
        cursor: pointer;
        transition: all 0.3s ease-in-out;
        z-index: 9500;
    }

        .glow-button2:hover {
            box-shadow: 0 0 15px #ff3333, 0 0 25px #ff3333, 0 0 40px #ff3333;
            transform: translate(-50%, -50%) scale(1.05);
        }

    .glow-button3 {
        position: absolute;
        top: 70%;
        left: 50%;
        width: 200px;
        height: 60px;
        font-family: 'Diablo';
        transform: translate(-50%, -50%);
        background: linear-gradient(145deg, #8b0000, #000000);
        color: darkred;
        font-size: 1.2rem;
        font-weight: bold;
        border: none;
        border-radius: 8px;
        box-shadow: 0 0 10px #ff0000, 0 0 20px #ff0000, 0 0 30px #ff0000;
        cursor: pointer;
        transition: all 0.3s ease-in-out;
        z-index: 9500;
    }

        .glow-button3:hover {
            box-shadow: 0 0 15px #ff3333, 0 0 25px #ff3333, 0 0 40px #ff3333;
            transform: translate(-50%, -50%) scale(1.05);
        }

    .glow-button4 {
        position: absolute;
        top: 80%;
        left: 50%;
        width: 200px;
        height: 60px;
        font-family: 'Diablo';
        transform: translate(-50%, -50%);
        background: linear-gradient(145deg, #8b0000, #000000);
        color: darkred;
        font-size: 1.2rem;
        font-weight: bold;
        border: none;
        border-radius: 8px;
        box-shadow: 0 0 10px #ff0000, 0 0 20px #ff0000, 0 0 30px #ff0000;
        cursor: pointer;
        transition: all 0.3s ease-in-out;
        z-index: 9500;
    }

        .glow-button4:hover {
            box-shadow: 0 0 15px #ff3333, 0 0 25px #ff3333, 0 0 40px #ff3333;
            transform: translate(-50%, -50%) scale(1.05);
        }
    .glow-button5 {
        position: absolute;
        top: 25%;
        left: 50%;
        width: 200px;
        height: 60px;
        font-family: 'Diablo';
        transform: translate(-50%, -50%);
        background: linear-gradient(145deg, #8b0000, #000000);
        color: darkred;
        font-size: 1.2rem;
        font-weight: bold;
        border: none;
        border-radius: 8px;
        box-shadow: 0 0 10px #ff0000, 0 0 20px #ff0000, 0 0 30px #ff0000;
        cursor: pointer;
        transition: all 0.3s ease-in-out;
        z-index: 9500;
    }

        .glow-button5:hover {
            box-shadow: 0 0 15px #ff3333, 0 0 25px #ff3333, 0 0 40px #ff3333;
            transform: translate(-50%, -50%) scale(1.05);
        }
    .glow-button6 {
        position: absolute;
        top: 35%;
        left: 50%;
        width: 200px;
        height: 60px;
        font-family: 'Diablo';
        transform: translate(-50%, -50%);
        background: linear-gradient(145deg, #8b0000, #000000);
        color: darkred;
        font-size: 1.2rem;
        font-weight: bold;
        border: none;
        border-radius: 8px;
        box-shadow: 0 0 10px #ff0000, 0 0 20px #ff0000, 0 0 30px #ff0000;
        cursor: pointer;
        transition: all 0.3s ease-in-out;
        z-index: 9500;
    }

        .glow-button6:hover {
            box-shadow: 0 0 15px #ff3333, 0 0 25px #ff3333, 0 0 40px #ff3333;
            transform: translate(-50%, -50%) scale(1.05);
        }
    .glow-button7 {
        position: absolute;
        top: 45%;
        left: 80%;
        width: 300px;
        height: 600px;
        font-family: 'Diablo';
        transform: translate(-50%, -50%);
        background-image: url('../iAssets/GothicFrameInv05.png');
        color: darkred;
        font-size: 1.2rem;
        font-weight: bold;
        border: none;
        border-radius: 8px;
        box-shadow: 0 0 10px #ff0000, 0 0 20px #ff0000, 0 0 30px #ff0000;
        cursor: pointer;
        transition: all 0.3s ease-in-out;
        z-index: 9500;
    }

        .glow-button7:hover {
            box-shadow: 0 0 15px #ff3333, 0 0 25px #ff3333, 0 0 40px #ff3333;
            transform: translate(-50%, -50%) scale(1.05);
        }
    .glow-button8 {
        position: absolute;
        top: 45%;
        left: 50%;
        width: 200px;
        height: 60px;
        font-family: 'Diablo';
        transform: translate(-50%, -50%);
        background: linear-gradient(145deg, #8b0000, #000000);
        color: darkred;
        font-size: 1.2rem;
        font-weight: bold;
        border: none;
        border-radius: 8px;
        box-shadow: 0 0 10px #ff0000, 0 0 20px #ff0000, 0 0 30px #ff0000;
        cursor: pointer;
        transition: all 0.3s ease-in-out;
        z-index: 9500;
    }

        .glow-button8:hover {
            box-shadow: 0 0 15px #ff3333, 0 0 25px #ff3333, 0 0 40px #ff3333;
            transform: translate(-50%, -50%) scale(1.05);
        }

    .char-button1 {
        position: absolute;
        top: 25%;
        left: 20%;
        width: 200px;
        height: 60px;
        font-family: 'Diablo';
        transform: translate(-50%, -50%);
        background: linear-gradient(145deg, #8b0000, #000000);
        color: darkred;
        font-size: 1.2rem;
        font-weight: bold;
        border: none;
        border-radius: 8px;
        box-shadow: 0 0 10px #ff0000, 0 0 20px #ff0000, 0 0 30px #ff0000;
        cursor: pointer;
        transition: all 0.3s ease-in-out;
        z-index: 9500;
    }

        .char-button1:hover {
            box-shadow: 0 0 15px #ff3333, 0 0 25px #ff3333, 0 0 40px #ff3333;
            transform: translate(-50%, -50%) scale(1.05);
        }
    .char-button2 {
        position: absolute;
        top: 35%;
        left: 20%;
        width: 200px;
        height: 60px;
        font-family: 'Diablo';
        transform: translate(-50%, -50%);
        background: linear-gradient(145deg, #8b0000, #000000);
        color: darkred;
        font-size: 1.2rem;
        font-weight: bold;
        border: none;
        border-radius: 8px;
        box-shadow: 0 0 10px #ff0000, 0 0 20px #ff0000, 0 0 30px #ff0000;
        cursor: pointer;
        transition: all 0.3s ease-in-out;
        z-index: 9500;
    }

        .char-button2:hover {
            box-shadow: 0 0 15px #ff3333, 0 0 25px #ff3333, 0 0 40px #ff3333;
            transform: translate(-50%, -50%) scale(1.05);
        }
    .char-button3 {
        position: absolute;
        top: 45%;
        left: 20%;
        width: 200px;
        height: 60px;
        font-family: 'Diablo';
        transform: translate(-50%, -50%);
        background: linear-gradient(145deg, #8b0000, #000000);
        color: darkred;
        font-size: 1.2rem;
        font-weight: bold;
        border: none;
        border-radius: 8px;
        box-shadow: 0 0 10px #ff0000, 0 0 20px #ff0000, 0 0 30px #ff0000;
        cursor: pointer;
        transition: all 0.3s ease-in-out;
        z-index: 900;
    }

        .char-button3:hover {
            box-shadow: 0 0 15px #ff3333, 0 0 25px #ff3333, 0 0 40px #ff3333;
            transform: translate(-50%, -50%) scale(1.05);
        }
    .char-button4 {
        position: absolute;
        top: 55%;
        left: 20%;
        width: 200px;
        height: 60px;
        font-family: 'Diablo';
        transform: translate(-50%, -50%);
        background: linear-gradient(145deg, #8b0000, #000000);
        color: darkred;
        font-size: 1.2rem;
        font-weight: bold;
        border: none;
        border-radius: 8px;
        box-shadow: 0 0 10px #ff0000, 0 0 20px #ff0000, 0 0 30px #ff0000;
        cursor: pointer;
        transition: all 0.3s ease-in-out;
        z-index: 9500;
    }

        .char-button4:hover {
            box-shadow: 0 0 15px #ff3333, 0 0 25px #ff3333, 0 0 40px #ff3333;
            transform: translate(-50%, -50%) scale(1.05);
        }
    .char-button5 {
        position: absolute;
        top: 65%;
        left: 20%;
        width: 200px;
        height: 60px;
        font-family: 'Diablo';
        transform: translate(-50%, -50%);
        background: linear-gradient(145deg, #8b0000, #000000);
        color: darkred;
        font-size: 1.2rem;
        font-weight: bold;
        border: none;
        border-radius: 8px;
        box-shadow: 0 0 10px #ff0000, 0 0 20px #ff0000, 0 0 30px #ff0000;
        cursor: pointer;
        transition: all 0.3s ease-in-out;
        z-index: 9500;
    }

        .char-button5:hover {
            box-shadow: 0 0 15px #ff3333, 0 0 25px #ff3333, 0 0 40px #ff3333;
            transform: translate(-50%, -50%) scale(1.05);
        }

        /*    remove colors etc      */
    .class-bar {
        display: flex;
        gap: 5px;
        position: absolute;
        top: 30px;
        left: 20px;
        right: 30px;
        bottom: 20px;
        font-family: 'Diablo';
        display: flex;
        font-size: 2rem;
        flex-direction: column;
        justify-content: flex-start;
        gap: 10px;
        z-index: 9500;
    }

    .level-bar {
        display: flex;
        gap: 5px;
        position: absolute;
        top: 60px;
        left: 20px;
        right: 30px;
        bottom: 20px;
        font-family: 'Diablo';
        display: flex;
        font-size: 2rem;
        flex-direction: column;
        justify-content: flex-start;
        gap: 10px;
        z-index: 9000;
    }

    .level-tick {
        width: 40px;
        height: 40px;
        text-align: center;
        line-height: 20px;
        font-size: 2rem;
        font-family: 'Digital';
        border-radius: 4px;
        padding-top: 2rem;
        z-index: 9500;
    }

        .level-tick.active {
            z-index: 9500;
        }

    .stat-row1 {
        gap: 5px;
        position: absolute;
        top: 330px;
        left: 20px;
        right: 30px;
        bottom: 20px;
        font-family: 'Digital';
        gap: 10px;
        font-size: 1.5rem;
        z-index: 9500;
    }

    .stat-row2 {
        position: absolute;
        top: 360px;
        left: 20px;
        right: 30px;
        bottom: 20px;
        font-family: 'Digital';
        font-size: 1.5rem;
        z-index: 9000;
    }

    .stat-row3 {
        gap: 5px;
        position: absolute;
        top: 390px;
        left: 20px;
        right: 30px;
        bottom: 20px;
        font-family: 'Digital';
        font-size: 1.5rem;
        z-index: 9500;
    }

    .stat-row4 {
        gap: 5px;
        position: absolute;
        top: 420px;
        left: 20px;
        right: 30px;
        bottom: 20px;
        font-family: 'Digital';
        font-size: 1.5rem;
        z-index: 9500;
    }

    .stat-row5 {
        gap: 5px;
        position: absolute;
        top: 450px;
        left: 20px;
        right: 30px;
        bottom: 20px;
        font-family: 'Digital';
        font-size: 1.5rem;
        z-index: 9500;
    }
    .stat-row6 {
        gap: 5px;
        position: absolute;
        top: 480px;
        left: 20px;
        right: 30px;
        bottom: 20px;
        font-family: 'Digital';
        font-size: 1.5rem;
        z-index: 9500;
    }
    .stat-row7 {
        gap: 5px;
        position: absolute;
        top: 510px;
        left: 20px;
        right: 30px;
        bottom: 20px;
        font-family: 'Digital';
        font-size: 1.5rem;
        z-index: 9500;
    }
    .stat-row8 {
        gap: 5px;
        position: absolute;
        top: 540px;
        left: 20px;
        right: 30px;
        bottom: 20px;
        font-family: 'Digital';
        font-size: 1.5rem;
        z-index: 9500;
    }
    .stat-row9 {
        gap: 5px;
        position: absolute;
        top: 570px;
        left: 20px;
        right: 30px;
        bottom: 20px;
        font-family: 'Digital';
        font-size: 1.5rem;
        z-index: 9500;
    }
    .stat-row10 {
        gap: 5px;
        position: absolute;
        top: 600px;
        left: 20px;
        right: 30px;
        bottom: 20px;
        font-family: 'Digital';
        font-size: 1.5rem;
        z-index: 9500;
    }

    .stat-row11 {
        gap: 5px;
        position: absolute;
        top: 330px;
        left: 250px;
        font-family: 'Digital';
        font-size: 1.5rem;
        z-index: 9500;
    }
    .xp-bar {
        display: flex;
        gap: 5px;
        position: absolute;
        top: 200px;
        left: 20px;
        right: 30px;
        bottom: 20px;
        font-family: 'Diablo';
        display: flex;
        font-size: 1.5rem;
        flex-direction: column;
        justify-content: flex-start;
        gap: 10px;
        z-index: 9500;
    }

    .xp-tick {
        width: 60px;
        height: 40px;
        background-color: #222;
        border: 1px solid #555;
        text-align: center;
        line-height: 20px;
        font-size: 1.5rem;
        font-family: 'Digital';
        border-radius: 4px;
        padding-top: 2rem;
        z-index: 9500;
    }

        .xp-tick.active {
        }
    .stat-plus {
        margin-left: 10px;
        color: white;
        border: none;
        padding: 2px 6px;
        font-weight: bold;
        cursor: pointer;
        border-radius: 4px;
        z-index: 9500;
    }

  





}













        ";
            return new MarkupString($"<style>{css}</style>");
        }


    }
}
