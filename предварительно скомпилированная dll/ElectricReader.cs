using HutongGames.PlayMaker;
using MSCLoader;
using System;
using UnityEngine;

namespace ElectricReader
{
    public class ElectricReader : Mod
    {
        private Camera cam;
        public float _kwh;
        public float _donedecimalless;
        public float _dunkwh;
        public float _done;
        private GameObject readere;
        public bool _firehouse;
        public float _bill;
        public bool _active;
        public Keybind key;
        public SettingsCheckBox my_checkbox;

        public override string ID => "ElectricReader"; //Your mod ID (unique)
        public override string Name => "Программа просмотра счетов за электроэнергию"; //You mod name
        public override string Author => "Jayy"; //Your Username
        public override string Version => "1.0r"; //Version
        public override string Description => "Просмотр счета за электроэнергию на русском языке."; //Short description of your mod

        public override void ModSetup()
        {
            this.SetupFunction(Mod.Setup.OnGUI, new Action(this.Mod_UI));
            this.SetupFunction(Mod.Setup.OnLoad, new Action(this.Mod_OnLoad));
            this.SetupFunction(Mod.Setup.OnSave, new Action(this.Mod_OnSave));
            this.SetupFunction(Mod.Setup.Update, new Action(this.Mod_Update));
        }

        public override void ModSettings()
        {
            Settings.AddHeader((Mod)this, "Настройки пользовательского интерфейса");
            this.my_checkbox = Settings.AddCheckBox((Mod)this, "my_checkbox", "Включить просмотр счетов в пользовательском интерфейсе", this._active);
        }

        private void Mod_OnLoad()
        {
            this._active = SaveLoad.ReadValue<bool>((Mod)this, "active");
            AssetBundle assetBundle = LoadAssets.LoadBundle((Mod)this, "elecreader.unity3d");
            this.readere = assetBundle.LoadAsset("elecreader.prefab") as GameObject;
            assetBundle.Unload(false);
            this.readere = UnityEngine.Object.Instantiate<GameObject>(this.readere);
            this.readere.name = "Reader";
            this.readere.transform.position = new Vector3(-11.948f, 0.900f, 8.8847f);
            this.readere.transform.localScale = new Vector3(0.4f, 0.4f, 0.4f);
            this.readere.transform.localRotation = Quaternion.Euler(90f, 0.0f, 0.0f);
        }

        private void Mod_OnSave() => SaveLoad.WriteValue<bool>((Mod)this, "active", this._active);

        public void Mod_Update()
        {
            foreach (FsmFloat floatVariable in PlayMakerGlobals.Instance.Variables.FloatVariables)
            {
                switch (floatVariable.Name)
                {
                    case "HouseElectricityKWH":
                        this._kwh = floatVariable.Value;
                        break;
                    case "HouseElectricity":
                        this._bill = floatVariable.Value;
                        break;
                }
                foreach (FsmBool boolVariable in PlayMakerGlobals.Instance.Variables.BoolVariables)
                {
                    if (boolVariable.Name == "HouseBurning")
                        this._firehouse = boolVariable.Value;
                    float kwh = this._kwh;
                    double num = 0.78;
                    this._done = kwh * (float)num;
                    this._donedecimalless = Mathf.Floor(this._done * 100f) / 100f;
                    this._dunkwh = Mathf.Floor(kwh * 100f) / 100f;
                }
            }
            if ((UnityEngine.Object)this.cam == (UnityEngine.Object)null)
            {
                this.cam = Camera.main;
            }
            else
            {
                foreach (RaycastHit raycastHit in Physics.RaycastAll(this.cam.transform.position, this.cam.transform.forward, 3f))
                {
                    if ((UnityEngine.Object)raycastHit.collider.gameObject == (UnityEngine.Object)this.readere)
                    {
                        PlayMakerGlobals.Instance.Variables.GetFsmBool("GUIuse").Value = true;
                        PlayMakerGlobals.Instance.Variables.GetFsmString("GUIinteraction").Value = string.Format("Schet za elektrichestvo: {1}mk", (object)this._dunkwh, (object)this._donedecimalless);
                    }
                }
            }
        }

        private void Mod_UI()
        {
            if (this.my_checkbox.GetValue())
            {
                GUI.Label(new Rect(5f, 125f, 2000f, 80f), string.Format("Счет за электроэнергию: {0}mk", (object)this._donedecimalless));
            }
            else
                GUI.enabled = false;
            if (this._firehouse)
                GUI.Label(new Rect(5f, 145f, 2000f, 80f), "О, блядь, твой дом горит!");
            if (!this.key.GetKeybindDown())
                return;
            if (this._active)
                GUI.enabled = false;
            else
                GUI.enabled = true;
        }
    }
}
