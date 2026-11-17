
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class AstravionBootstrap : MonoBehaviour
{
    class UnitData { public GameObject go; public float hp; public float cd; public bool red; public bool minion; }
    class DragonData { public GameObject go; public float life; public Vector3 dir; }

    Material grass, lane, stone, water, blue, red, gold, dark, armor, skin, fire, leaf;
    GameObject hero, cameraRig, blueCore, redCore, joystickKnob;
    Transform cameraTransform;
    Canvas canvas;
    Text timerText, scoreText, hpText, manaText, goldText, levelText, noticeText;
    Image hpFill, manaFill;
    readonly List<UnitData> enemies = new List<UnitData>();
    readonly List<UnitData> minions = new List<UnitData>();
    readonly List<DragonData> dragons = new List<DragonData>();
    Vector2 moveInput;
    float hp = 100, mana = 100, goldCount = 0, elapsed = 0, spawnClock = 3;
    float attackCd, dashCd, arcCd, ultCd;
    int blueScore, redScore, level = 1;
    bool started, paused, ended;

    void Start()
    {
        Application.targetFrameRate = 60;
        Screen.orientation = ScreenOrientation.LandscapeLeft;
        MakeMaterials();
        BuildArena();
        BuildHero();
        BuildCamera();
        BuildUI();
        SpawnWave();
        started = true;
        SetNotice("ASTRAVION siap. Gunakan joystick untuk bergerak.");
    }

    void MakeMaterials()
    {
        grass = MakeMat(new Color(.12f,.34f,.20f));
        lane = MakeMat(new Color(.48f,.43f,.32f));
        stone = MakeMat(new Color(.27f,.31f,.37f), .15f);
        water = MakeMat(new Color(.05f,.28f,.43f), .45f);
        blue = MakeMat(new Color(.12f,.40f,.88f), .35f);
        red = MakeMat(new Color(.72f,.08f,.16f), .35f);
        gold = MakeMat(new Color(1f,.66f,.12f), .7f);
        dark = MakeMat(new Color(.055f,.065f,.10f), .25f);
        armor = MakeMat(new Color(.55f,.08f,.12f), .5f);
        skin = MakeMat(new Color(.78f,.49f,.32f));
        fire = MakeMat(new Color(1f,.12f,.015f), .2f, true);
        leaf = MakeMat(new Color(.035f,.23f,.12f));
    }

    Material MakeMat(Color c, float metallic = 0f, bool emission = false)
    {
        Shader s = Shader.Find("Standard");
        Material m = new Material(s);
        m.color = c;
        m.SetFloat("_Metallic", metallic);
        m.SetFloat("_Glossiness", .38f);
        if (emission) { m.EnableKeyword("_EMISSION"); m.SetColor("_EmissionColor", c * 2.2f); }
        return m;
    }

    GameObject Part(PrimitiveType type, string n, Transform parent, Vector3 pos, Vector3 scale, Material mat)
    {
        GameObject g = GameObject.CreatePrimitive(type);
        g.name = n;
        g.transform.SetParent(parent, false);
        g.transform.localPosition = pos;
        g.transform.localScale = scale;
        Renderer r = g.GetComponent<Renderer>();
        if (r) r.sharedMaterial = mat;
        Collider c = g.GetComponent<Collider>();
        if (c) Destroy(c);
        return g;
    }

    void BuildArena()
    {
        Part(PrimitiveType.Cube, "Battlefield Grass", transform, new Vector3(0,-.7f,0), new Vector3(112,1,82), grass);
        for (int zLane = -1; zLane <= 1; zLane++)
        {
            float z = zLane * 24f;
            Part(PrimitiveType.Cube, "Stone Lane", transform, new Vector3(0,-.12f,z), new Vector3(108,.18f,10), lane);
            for (int x = -48; x <= 48; x += 8)
                Part(PrimitiveType.Cube, "Lane Paving", transform, new Vector3(x,-.015f,z), new Vector3(.12f,.04f,9.6f), stone);
        }
        Part(PrimitiveType.Cube, "River", transform, new Vector3(0,-.18f,0), new Vector3(108,.16f,5), water);
        for (int side = -1; side <= 1; side += 2)
        {
            for (int i = 0; i < 12; i++)
            {
                float x = -45 + i * 8.2f;
                float z = side * (9f + (i % 3) * 3f);
                if (Mathf.Abs(z) < 13) z = side * 17;
                MakeTree(new Vector3(x,0,z));
            }
        }
        MakeBase(-49, blue);
        MakeBase(49, red);
        MakeTower(-28,-24,blue); MakeTower(-28,0,blue); MakeTower(-28,24,blue);
        MakeTower(28,-24,red); MakeTower(28,0,red); MakeTower(28,24,red);
        blueCore = MakeCore(-49, blue);
        redCore = MakeCore(49, red);
        RenderSettings.ambientLight = new Color(.62f,.67f,.72f);
        GameObject sun = new GameObject("ASTRAVION Sun");
        Light l = sun.AddComponent<Light>(); l.type = LightType.Directional; l.intensity = 1.15f;
        sun.transform.rotation = Quaternion.Euler(48,-32,0);
    }

    void MakeTree(Vector3 p)
    {
        GameObject t = new GameObject("Jungle Tree"); t.transform.SetParent(transform);
        Part(PrimitiveType.Cylinder,"Trunk",t.transform,new Vector3(0,1.4f,0),new Vector3(.42f,1.5f,.42f),lane);
        Part(PrimitiveType.Sphere,"Canopy lower",t.transform,new Vector3(0,3.2f,0),new Vector3(3.2f,3.6f,3.2f),leaf);
        Part(PrimitiveType.Sphere,"Canopy upper",t.transform,new Vector3(0,4.6f,0),new Vector3(2.2f,2.7f,2.2f),leaf);
        t.transform.position = p;
    }

    void MakeBase(float x, Material team)
    {
        GameObject b = new GameObject("Team Base"); b.transform.SetParent(transform);
        Part(PrimitiveType.Cylinder,"Base platform",b.transform,Vector3.zero,new Vector3(13,.7f,16),stone);
        Part(PrimitiveType.Cylinder,"Base energy ring",b.transform,new Vector3(0,.48f,0),new Vector3(9,.18f,11),gold);
        b.transform.position = new Vector3(x,0,0);
    }

    void MakeTower(float x,float z,Material team)
    {
        GameObject t = new GameObject("Defensive Tower"); t.transform.SetParent(transform);
        Part(PrimitiveType.Cylinder,"Tower base",t.transform,new Vector3(0,.65f,0),new Vector3(4.2f,1.3f,4.2f),stone);
        Part(PrimitiveType.Cylinder,"Tower body",t.transform,new Vector3(0,3.1f,0),new Vector3(2.5f,4.3f,2.5f),team);
        Part(PrimitiveType.Cylinder,"Tower crown",t.transform,new Vector3(0,5.9f,0),new Vector3(3.1f,2.4f,3.1f),gold);
        Part(PrimitiveType.Sphere,"Tower crystal",t.transform,new Vector3(0,7.35f,0),new Vector3(1.1f,1.1f,1.1f),team);
        t.transform.position = new Vector3(x,0,z);
    }

    GameObject MakeCore(float x, Material team)
    {
        GameObject c = new GameObject("Ancient Core"); c.transform.SetParent(transform);
        Part(PrimitiveType.Cylinder,"Core dais",c.transform,new Vector3(0,.5f,0),new Vector3(8,1,8),gold);
        Part(PrimitiveType.Sphere,"Core crystal",c.transform,new Vector3(0,3.2f,0),new Vector3(3.2f,4.2f,3.2f),team);
        Part(PrimitiveType.Cube,"Core armor",c.transform,new Vector3(0,3.2f,0),new Vector3(5,.8f,5),stone);
        c.transform.position = new Vector3(x,0,0);
        return c;
    }

    void BuildHero()
    {
        hero = new GameObject("KAEL - Crimson Warden");
        hero.transform.position = new Vector3(-39,0,0);
        hero.transform.rotation = Quaternion.LookRotation(Vector3.right);
        Part(PrimitiveType.Capsule,"Body armor",hero.transform,new Vector3(0,1.25f,0),new Vector3(.95f,1.25f,.72f),dark);
        Part(PrimitiveType.Cube,"Crimson chestplate",hero.transform,new Vector3(.05f,1.75f,0),new Vector3(1.05f,.72f,.82f),armor);
        Part(PrimitiveType.Sphere,"Head",hero.transform,new Vector3(0,2.55f,0),new Vector3(.68f,.72f,.65f),skin);
        Part(PrimitiveType.Sphere,"War helmet",hero.transform,new Vector3(0,2.83f,0),new Vector3(.78f,.42f,.72f),dark);
        Part(PrimitiveType.Cylinder,"Left helmet horn",hero.transform,new Vector3(-.38f,3.12f,0),new Vector3(.22f,.8f,.22f),gold).transform.localRotation=Quaternion.Euler(0,0,28);
        Part(PrimitiveType.Cylinder,"Right helmet horn",hero.transform,new Vector3(.38f,3.12f,0),new Vector3(.22f,.8f,.22f),gold).transform.localRotation=Quaternion.Euler(0,0,-28);
        Part(PrimitiveType.Cube,"Red cape",hero.transform,new Vector3(-.42f,1.35f,-.48f),new Vector3(.9f,1.55f,.12f),red);
        Part(PrimitiveType.Sphere,"Left shoulder",hero.transform,new Vector3(-.72f,2.05f,0),new Vector3(.58f,.55f,.62f),gold);
        Part(PrimitiveType.Sphere,"Right shoulder",hero.transform,new Vector3(.72f,2.05f,0),new Vector3(.58f,.55f,.62f),gold);
        Part(PrimitiveType.Cylinder,"Left leg",hero.transform,new Vector3(-.28f,.48f,0),new Vector3(.32f,.9f,.35f),dark);
        Part(PrimitiveType.Cylinder,"Right leg",hero.transform,new Vector3(.28f,.48f,0),new Vector3(.32f,.9f,.35f),dark);
        GameObject sword = Part(PrimitiveType.Cube,"Flame Sword",hero.transform,new Vector3(.98f,1.25f,.12f),new Vector3(.18f,2.2f,.28f),fire);
        sword.transform.localRotation = Quaternion.Euler(0,0,-18);
        Part(PrimitiveType.Cube,"Sword guard",hero.transform,new Vector3(.94f,.55f,.12f),new Vector3(.85f,.16f,.34f),gold);
    }

    void BuildCamera()
    {
        GameObject cam = new GameObject("MOBA Follow Camera");
        Camera c = cam.AddComponent<Camera>();
        c.fieldOfView = 48; c.nearClipPlane=.1f; c.farClipPlane=180;
        cam.AddComponent<AudioListener>();
        cameraTransform = cam.transform;
        cameraTransform.position = hero.transform.position + new Vector3(0,25,-23);
        cameraTransform.rotation = Quaternion.LookRotation(hero.transform.position + Vector3.up - cameraTransform.position);
    }

    void BuildUI()
    {
        GameObject canvasObj = new GameObject("ASTRAVION Mobile HUD",typeof(Canvas),typeof(CanvasScaler),typeof(GraphicRaycaster));
        canvas = canvasObj.GetComponent<Canvas>(); canvas.renderMode=RenderMode.ScreenSpaceOverlay;
        CanvasScaler scaler=canvasObj.GetComponent<CanvasScaler>(); scaler.uiScaleMode=CanvasScaler.ScaleMode.ScaleWithScreenSize; scaler.referenceResolution=new Vector2(1920,1080); scaler.matchWidthOrHeight=.5f;
        if (FindObjectOfType<EventSystem>()==null) { GameObject es=new GameObject("EventSystem",typeof(EventSystem),typeof(StandaloneInputModule)); }
        MakeText("Brand","ASTRAVION  |  LEGENDS OF ETERNAL WAR",new Vector2(.5f,1),new Vector2(.5f,1),new Vector2(0,-35),new Vector2(600,55),26,new Color(1f,.78f,.32f));
        timerText=MakeText("Timer","00:00",new Vector2(.5f,1),new Vector2(.5f,1),new Vector2(0,-88),new Vector2(180,38),22,Color.white);
        scoreText=MakeText("Score","BLUE 0     VS     0 RED",new Vector2(.5f,1),new Vector2(.5f,1),new Vector2(0,-125),new Vector2(360,38),18,Color.white);
        hpText=MakeText("HP","KAEL  ·  HP 100",new Vector2(0,1),new Vector2(0,1),new Vector2(24,-45),new Vector2(300,35),20,Color.white);
        hpFill=MakeBar("HP Bar",new Vector2(0,1),new Vector2(0,1),new Vector2(24,-85),new Vector2(280,16),new Color(.1f,.9f,.35f));
        manaText=MakeText("Mana","MANA 100",new Vector2(0,1),new Vector2(0,1),new Vector2(24,-112),new Vector2(280,30),16,new Color(.55f,.8f,1));
        manaFill=MakeBar("Mana Bar",new Vector2(0,1),new Vector2(0,1),new Vector2(24,-143),new Vector2(280,12),new Color(.2f,.55f,1));
        goldText=MakeText("Gold","GOLD 0",new Vector2(0,1),new Vector2(0,1),new Vector2(24,-174),new Vector2(220,28),15,gold.color);
        levelText=MakeText("Level","LEVEL 1",new Vector2(0,1),new Vector2(0,1),new Vector2(24,-204),new Vector2(220,28),15,Color.white);
        noticeText=MakeText("Notice","",new Vector2(.5f,0),new Vector2(.5f,0),new Vector2(0,120),new Vector2(700,42),18,Color.white);
        MakeButton("ATTACK", "⚔\\nSERANG",new Vector2(1,0),new Vector2(1,0),new Vector2(-115,145),new Vector2(120,120),new Color(.3f,.22f,.18f),BasicAttack,22);
        MakeButton("DASH","🔥\\nDASH",new Vector2(1,0),new Vector2(1,0),new Vector2(-255,105),new Vector2(100,100),new Color(.55f,.16f,.08f),Dash,20);
        MakeButton("FLAME ARC","✦\\nFLAME ARC",new Vector2(1,0),new Vector2(1,0),new Vector2(-375,105),new Vector2(100,100),new Color(.42f,.13f,.12f),FlameArc,17);
        MakeButton("ULTIMATE","🐉\\nDRAGON WRATH",new Vector2(1,0),new Vector2(1,0),new Vector2(-510,130),new Vector2(120,120),new Color(.7f,.12f,.06f),Ultimate,17);
        MakeButton("PAUSE","Ⅱ",new Vector2(1,1),new Vector2(1,1),new Vector2(-55,-55),new Vector2(80,65),new Color(.08f,.12f,.2f),TogglePause,24);
        GameObject joy=MakePanel("Joystick",new Vector2(0,0),new Vector2(0,0),new Vector2(135,145),new Vector2(190,190),new Color(.12f,.25f,.4f,.65f));
        joystickKnob=MakePanel("Joystick Knob",new Vector2(.5f,.5f),new Vector2(.5f,.5f),Vector2.zero,new Vector2(78,78),new Color(.8f,.88f,1f,.9f),joy.transform);
        EventTrigger trigger=joy.AddComponent<EventTrigger>();
        AddTrigger(trigger,EventTriggerType.PointerDown,OnJoy);
        AddTrigger(trigger,EventTriggerType.Drag,OnJoy);
        AddTrigger(trigger,EventTriggerType.PointerUp,OnJoyEnd);
        AddTrigger(trigger,EventTriggerType.PointerExit,OnJoyEnd);
    }

    void AddTrigger(EventTrigger t, EventTriggerType type, System.Action<BaseEventData> action)
    {
        EventTrigger.Entry e=new EventTrigger.Entry(); e.eventID=type; e.callback.AddListener((d)=>action(d)); t.triggers.Add(e);
    }
    void OnJoy(BaseEventData data)
    {
        PointerEventData p=data as PointerEventData; if(p==null)return;
        RectTransform rt=joystickKnob.transform.parent as RectTransform;
        Vector2 local; RectTransformUtility.ScreenPointToLocalPointInRectangle(rt,p.position,p.pressEventCamera,out local);
        float radius=rt.rect.width*.32f; moveInput=Vector2.ClampMagnitude(local/radius,1);
        joystickKnob.GetComponent<RectTransform>().anchoredPosition=moveInput*radius;
    }
    void OnJoyEnd(BaseEventData data){moveInput=Vector2.zero;if(joystickKnob)joystickKnob.GetComponent<RectTransform>().anchoredPosition=Vector2.zero;}

    GameObject MakePanel(string n,Vector2 amin,Vector2 amax,Vector2 pos,Vector2 size,Color color,Transform parent=null)
    {
        GameObject g=new GameObject(n,typeof(RectTransform),typeof(Image));g.transform.SetParent(parent?parent:canvas.transform,false);
        RectTransform r=g.GetComponent<RectTransform>();r.anchorMin=amin;r.anchorMax=amax;r.anchoredPosition=pos;r.sizeDelta=size;
        g.GetComponent<Image>().color=color;return g;
    }
    Text MakeText(string n,string value,Vector2 amin,Vector2 amax,Vector2 pos,Vector2 size,int font,Color color)
    {
        GameObject g=new GameObject(n,typeof(RectTransform),typeof(Text));g.transform.SetParent(canvas.transform,false);
        RectTransform r=g.GetComponent<RectTransform>();r.anchorMin=amin;r.anchorMax=amax;r.anchoredPosition=pos;r.sizeDelta=size;
        Text t=g.GetComponent<Text>();t.text=value;t.font=Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");t.fontSize=font;t.color=color;t.alignment=TextAnchor.MiddleCenter;t.fontStyle=FontStyle.Bold;return t;
    }
    Image MakeBar(string n,Vector2 amin,Vector2 amax,Vector2 pos,Vector2 size,Color color)
    {
        GameObject bg=MakePanel(n+" BG",amin,amax,pos,size,new Color(.04f,.06f,.09f,.9f));
        GameObject fill=MakePanel(n,Vector2.zero,new Vector2(1,1),Vector2.zero,size,color,bg.transform);
        fill.GetComponent<RectTransform>().pivot=new Vector2(0,0.5f);
        return fill.GetComponent<Image>();
    }
    void MakeButton(string n,string label,Vector2 amin,Vector2 amax,Vector2 pos,Vector2 size,Color color,UnityEngine.Events.UnityAction action,int font)
    {
        GameObject g=MakePanel(n,amin,amax,pos,size,color);
        Button b=g.AddComponent<Button>();b.onClick.AddListener(action);
        GameObject tx=new GameObject("Label",typeof(RectTransform),typeof(Text));tx.transform.SetParent(g.transform,false);
        RectTransform r=tx.GetComponent<RectTransform>();r.anchorMin=Vector2.zero;r.anchorMax=Vector2.one;r.offsetMin=Vector2.zero;r.offsetMax=Vector2.zero;
        Text t=tx.GetComponent<Text>();t.text=label;t.font=Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");t.fontSize=font;t.fontStyle=FontStyle.Bold;t.alignment=TextAnchor.MiddleCenter;t.color=Color.white;
    }

    void SpawnWave()
    {
        for(int i=0;i<3;i++){UnitData e=MakeUnit(true,false);e.go.transform.position=new Vector3(35,0,(i-1)*2.2f);enemies.Add(e);}
        for(int i=0;i<3;i++){UnitData m=MakeUnit(false,true);m.go.transform.position=new Vector3(-42-i*1.8f,(0),(i-1)*2f);minions.Add(m);}
    }
    UnitData MakeUnit(bool isRed,bool isMinion)
    {
        GameObject g=new GameObject(isRed?(isMinion?"Red Minion":"Red Bot"):"Blue Minion");
        Material team=isRed?red:blue;
        Part(PrimitiveType.Capsule,"Armor",g.transform,new Vector3(0,isMinion?.7f:1f,0),isMinion?new Vector3(.65f,.85f,.65f):new Vector3(.9f,1.25f,.8f),team);
        Part(PrimitiveType.Sphere,"Head",g.transform,new Vector3(0,isMinion?1.35f:1.95f,0),new Vector3(.55f,.55f,.55f),skin);
        Part(PrimitiveType.Cube,"Weapon",g.transform,new Vector3(.55f,1f,0),new Vector3(.15f,1.25f,.18f),gold);
        return new UnitData{go=g,hp=isMinion?65:130,red=isRed,minion=isMinion};
    }

    void BasicAttack()
    {
        if(!started||paused||attackCd>0)return;attackCd=.48f;
        UnitData target=NearestEnemy(7f);
        if(target!=null){target.hp-=38;HitFx(target.go.transform.position+Vector3.up,fire);if(target.hp<=0)Kill(target);}
        else if(Vector3.Distance(hero.transform.position,redCore.transform.position)<14f){HitFx(redCore.transform.position+Vector3.up*3,fire);redCore.transform.localScale*=.92f;}
    }
    UnitData NearestEnemy(float max)
    {
        UnitData best=null;float dmax=max;
        foreach(UnitData e in enemies){if(e.hp<=0)continue;float d=Vector3.Distance(hero.transform.position,e.go.transform.position);if(d<dmax){dmax=d;best=e;}}
        return best;
    }
    void Kill(UnitData e){blueScore++;goldCount+=35;level=1+blueScore/3;Destroy(e.go);}
    void Dash()
    {
        if(!started||paused||dashCd>0||mana<18)return;dashCd=4;mana-=18;
        Vector3 dir=hero.transform.forward;if(moveInput.sqrMagnitude>.05f)dir=new Vector3(moveInput.x,0,moveInput.y).normalized;
        hero.transform.position+=dir*9;HitFx(hero.transform.position,fire);
        foreach(UnitData e in enemies)if(e.hp>0&&Vector3.Distance(e.go.transform.position,hero.transform.position)<6){e.hp-=48;if(e.hp<=0)Kill(e);}
    }
    void FlameArc()
    {
        if(!started||paused||arcCd>0||mana<25)return;arcCd=7;mana-=25;HitFx(hero.transform.position+Vector3.up,fire,7);
        foreach(UnitData e in enemies)if(e.hp>0&&Vector3.Distance(e.go.transform.position,hero.transform.position)<11){e.hp-=62;if(e.hp<=0)Kill(e);}
    }
    void Ultimate()
    {
        if(!started||paused||ultCd>0||mana<45)return;ultCd=18;mana-=45;
        GameObject dragon=BuildDragon();dragon.transform.position=hero.transform.position+Vector3.up*2+hero.transform.forward*2;
        dragons.Add(new DragonData{go=dragon,life=2.8f,dir=hero.transform.forward});
        foreach(UnitData e in enemies)if(e.hp>0&&Vector3.Distance(e.go.transform.position,hero.transform.position)<18){e.hp-=125;if(e.hp<=0)Kill(e);}
        SetNotice("DRAGON WRATH! Naga api KAEL menyerang!");
    }
    GameObject BuildDragon()
    {
        GameObject d=new GameObject("KAEL RED FIRE DRAGON");
        Material dragonMat=MakeMat(new Color(1f,.055f,.015f),.25f,true);
        for(int i=0;i<7;i++)Part(PrimitiveType.Sphere,"Flame Dragon Body",d.transform,new Vector3(-i*.85f,Mathf.Sin(i*.55f)*.35f,0),new Vector3(1.15f-i*.07f,.85f-i*.035f,.9f-i*.04f),dragonMat);
        Part(PrimitiveType.Sphere,"Dragon Head",d.transform,new Vector3(1.1f,.15f,0),new Vector3(1.35f,1.05f,1.15f),dragonMat);
        Part(PrimitiveType.Cylinder,"Dragon Horn L",d.transform,new Vector3(1.15f,.85f,-.48f),new Vector3(.3f,1.05f,.3f),gold).transform.localRotation=Quaternion.Euler(0,0,-35);
        Part(PrimitiveType.Cylinder,"Dragon Horn R",d.transform,new Vector3(1.15f,.85f,.48f),new Vector3(.3f,1.05f,.3f),gold).transform.localRotation=Quaternion.Euler(0,0,35);
        Part(PrimitiveType.Sphere,"Dragon Flame Breath",d.transform,new Vector3(2f,-.05f,0),new Vector3(.9f,2.4f,.9f),fire).transform.localRotation=Quaternion.Euler(0,0,-90);
        for(int s=-1;s<=1;s+=2){GameObject wing=Part(PrimitiveType.Cube,"Dragon Wing",d.transform,new Vector3(-.2f,.7f,s*1.05f),new Vector3(1.5f,.12f,2.4f),dragonMat);wing.transform.localRotation=Quaternion.Euler(s*20,0,s*25);}
        return d;
    }
    void HitFx(Vector3 pos,Material m,float scale=1.8f)
    {
        GameObject fx=GameObject.CreatePrimitive(PrimitiveType.Sphere);fx.name="Impact FX";fx.transform.position=pos;fx.transform.localScale=Vector3.one*scale;
        Renderer r=fx.GetComponent<Renderer>();r.sharedMaterial=m;Destroy(fx.GetComponent<Collider>());Destroy(fx, .35f);
    }

    void TogglePause(){paused=!paused;SetNotice(paused?"PERTANDINGAN DIJEDA":"PERTANDINGAN DILANJUTKAN");}
    void SetNotice(string s){if(noticeText)noticeText.text=s;}

    void Update()
    {
        if(!started||paused||ended)return;
        float dt=Time.deltaTime;elapsed+=dt;spawnClock-=dt;
        if(spawnClock<=0){SpawnWave();spawnClock=14;}
        attackCd=Mathf.Max(0,attackCd-dt);dashCd=Mathf.Max(0,dashCd-dt);arcCd=Mathf.Max(0,arcCd-dt);ultCd=Mathf.Max(0,ultCd-dt);
        mana=Mathf.Min(100,mana+5*dt);
        Vector3 move=new Vector3(moveInput.x,0,moveInput.y);
        if(move.sqrMagnitude>.01f){hero.transform.position+=move.normalized*10*dt;hero.transform.rotation=Quaternion.LookRotation(move);}
        hero.transform.position=new Vector3(Mathf.Clamp(hero.transform.position.x,-52,52),0,Mathf.Clamp(hero.transform.position.z,-36,36));
        foreach(UnitData e in enemies)
        {
            if(e.hp<=0||!e.go)continue;
            Vector3 to=hero.transform.position-e.go.transform.position;to.y=0;float dist=to.magnitude;
            if(dist>2.8f){to.Normalize();e.go.transform.position+=to*3.1f*dt;e.go.transform.rotation=Quaternion.LookRotation(to);}
            else {e.cd-=dt;if(e.cd<=0){hp-=7;e.cd=1.1f;}}
        }
        foreach(UnitData m in minions)
        {
            if(m.hp<=0||!m.go)continue;m.go.transform.position+=Vector3.right*3.2f*dt;
            foreach(UnitData e in enemies){if(e.hp>0&&e.go&&Vector3.Distance(m.go.transform.position,e.go.transform.position)<4){e.hp-=12*dt;break;}}
        }
        for(int i=dragons.Count-1;i>=0;i--){DragonData d=dragons[i];if(!d.go){dragons.RemoveAt(i);continue;}d.life-=dt;d.go.transform.position+=d.dir*18*dt;d.go.transform.Rotate(0,0,90*dt);if(d.life<=0){Destroy(d.go);dragons.RemoveAt(i);}}
        if(hp<=0){hp=100;redScore++;hero.transform.position=new Vector3(-39,0,0);SetNotice("KAEL tumbang. Kembali ke markas!");}
        if(redCore&&redCore.transform.localScale.x<.35f){ended=true;SetNotice("KEMENANGAN ASTRAVION!");}
        if(cameraTransform){Vector3 desired=hero.transform.position+new Vector3(0,25,-23);cameraTransform.position=Vector3.Lerp(cameraTransform.position,desired,dt*4);cameraTransform.rotation=Quaternion.Slerp(cameraTransform.rotation,Quaternion.LookRotation(hero.transform.position+Vector3.up-cameraTransform.position),dt*5);}
        RefreshHUD();
    }
    void RefreshHUD()
    {
        if(timerText)timerText.text=string.Format("{0:00}:{1:00}",Mathf.FloorToInt(elapsed/60),Mathf.FloorToInt(elapsed%60));
        if(scoreText)scoreText.text="BLUE "+blueScore+"     VS     "+redScore+" RED";
        if(hpText)hpText.text="KAEL  ·  HP "+Mathf.CeilToInt(hp);
        if(manaText)manaText.text="MANA "+Mathf.CeilToInt(mana);
        if(hpFill)hpFill.rectTransform.anchorMax=new Vector2(Mathf.Clamp01(hp/100f),1);
        if(manaFill)manaFill.rectTransform.anchorMax=new Vector2(Mathf.Clamp01(mana/100f),1);
        if(goldText)goldText.text="GOLD "+Mathf.FloorToInt(goldCount);
        if(levelText)levelText.text="LEVEL "+level;
    }
    public void BeginMatch(){started=true;paused=false;ended=false;SetNotice("Hancurkan Ancient Core merah!");}
}
