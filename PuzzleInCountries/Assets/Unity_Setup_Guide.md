# Unity Enhanced Third Person Controller - Kurulum Rehberi

## 🎯 Bu Rehber Size Şunları Öğretecek:
- Unity'de sıfırdan character controller kurulumu
- Input System yapılandırması
- Animasyon sistemi kurulumu
- Prefab ve component ayarları
- Mobile UI entegrasyonu

---

## 📋 ADIM 1: PROJENİZİ HAZIRLAYIN

### 1.1 Unity Versiyonu
- **Unity 2021.3 LTS** veya daha yeni bir versiyon kullanın
- **Built-in Render Pipeline** veya **URP** ile uyumludur

### 1.2 Gerekli Package'lar
**Window → Package Manager** açın ve şunları install edin:
- ✅ **Input System** (com.unity.inputsystem)

### 1.3 Project Settings
**Edit → Project Settings** açın:
- **XR Plug-in Management → Initialize XR on Startup** = OFF
- **Input System Package (New)** seçin
- Unity'yi restart edin

---

## 📋 ADIM 2: KARAKTER PREFAB OLUŞTURUN

### 2.1 Temel GameObject Oluştur
1. **Hierarchy** → sağ tık → **Create Empty** → isim: `Player`
2. **Transform** pozisyonunu `(0, 0, 0)` yapın

### 2.2 Karakter Modelini Ekleyin
1. Karakter 3D modelinizi **Player** objesinin altına çocuk olarak ekleyin
2. Model pozisyonunu `(0, 0, 0)` yapın
3. Model'in **Animator** component'i olduğundan emin olun

### 2.3 CharacterController Ekleyin
**Player** objesini seçin → **Add Component**:
- **CharacterController**
  - Center: `(0, 1, 0)`
  - Radius: `0.28`
  - Height: `1.8`
  - Skin Width: `0.08`
  - Min Move Distance: `0.001`

**⚠️ Önemli**: Radius değeri, EnhancedThirdPersonController'daki GroundedRadius ile aynı olmalı!

---

## 📋 ADIM 3: INPUT SYSTEM KURULUMU

### 3.1 Input Actions Oluşturun
1. **Assets** klasöründe sağ tık → **Create → Input Actions**
2. İsim: `PlayerInputActions`
3. Çift tıklayarak açın

### 3.2 Action Map ve Actions
**Player** action map'inde şu action'ları oluşturun:

| Action Name | Action Type | Control Type |
|-------------|-------------|--------------|
| Move        | Value       | Vector2      |
| Look        | Value       | Vector2      |
| Jump        | Button      | Button       |
| Sprint      | Button      | Button       |
| Interact    | Button      | Button       |
| CreateClone | Button      | Button       |

### 3.3 Bindings Ekleyin
Her action için binding'leri ekleyin:

**Move:**
- WASD (2D Vector Composite)
- Left Stick (Gamepad)

**Look:**
- Mouse Delta
- Right Stick (Gamepad)

**Jump:**
- Space (Keyboard)
- South Button (Gamepad)

**Sprint:**
- Left Shift (Keyboard)
- Left Shoulder (Gamepad)

**Interact:**
- E (Keyboard)
- West Button (Gamepad)

**CreateClone:**
- Q (Keyboard)
- North Button (Gamepad)

### 3.4 Input Actions Save
- **Save Asset** butonuna tıklayın
- **Generate C# Class** edin

---

## 📋 ADIM 4: CONTROLLER COMPONENTS

### 4.1 Player Input Component
**Player** objesine **Add Component**:
- **Player Input**
  - Actions: `PlayerInputActions` asset'ini sürükleyin
  - Default Map: `Player`
  - Behavior: `Send Messages`

### 4.2 StarterAssetsInputs
**Player** objesine **Add Component**:
- Script'i bulun: `StarterAssetsInputs`

### 4.3 PositionTracker
**Player** objesine **Add Component**:
- Script'i bulun: `PositionTracker`

### 4.4 EnhancedThirdPersonController
**Player** objesine **Add Component**:
- Script'i bulun: `EnhancedThirdPersonController`

---

## 📋 ADIM 5: KAMERA KURULUMU

### 5.1 CinemachineStyleCamera
1. **Main Camera** objesini seçin
2. **Add Component** → `CinemachineStyleCamera` script

### 5.2 Kamera Ayarları
**CinemachineStyleCamera** ayarları:
- **Default Target**: `Player` objesini sürükleyin
- **Default Offset**: `(5, 2, 5)` (çapraz kamera pozisyonu)
- **Default Follow Damping**: `1.0` (takip yumuşatma)
- **Default Look Damping**: `1.0` (bakış yumuşatma)
- **Field Of View**: `60` (kamera görüş açısı)

### 5.3 Çarpışma Ayarları
**Collision** ayarları:
- **Collision Layers**: Duvar/Engel layer'larını seçin
- **Collision Offset**: `0.2` (duvar mesafesi)

### 5.4 Otomatik Target Bulma
Script otomatik olarak "Player" tag'li objeyi bulur, manual atama gerekmez.

---

## 📋 ADIM 6: ANIMASYON KURULUMU

### 6.1 Animator Controller
1. **Assets** → sağ tık → **Create → Animator Controller**
2. İsim: `PlayerAnimatorController`

### 6.2 Animasyon Parameters
Animator Controller'ı açın → **Parameters** tab:
- `Speed` (Float)
- `Grounded` (Bool)  
- `Jump` (Bool)
- `FreeFall` (Bool)
- `MotionSpeed` (Float)

### 6.3 Animasyon States
Temel animasyon state'leri oluşturun:
- **Idle** (varsayılan)
- **Walk/Run** (Speed > 0.1)
- **Jump** (Jump trigger)
- **Fall** (FreeFall = true)

### 6.4 Animator Component
**Player** modelinde **Animator**:
- **Controller**: `PlayerAnimatorController` sürükleyin

---

## 📋 ADIM 7: ZEMIN KURULUMU

### 7.1 Basit Zemin Oluştur
1. **3D Object → Plane** → isim: `Ground`
2. Pozisyon: `(0, 0, 0)`
3. Scale: `(10, 1, 10)` (istediğiniz kadar büyük)
4. **Box Collider** olduğundan emin olun

### 7.2 Test
Play butonuna basın - karakter artık zeminde durmalı!

---

## 📋 ADIM 8: ETKİLEŞİM SİSTEMLERİ

### 8.1 ModernButtonController
**Interaktif butonlar oluşturmak için:**

1. **3D Object → Cube** → isim: `InteractiveButton`
2. Scale: `(1, 0.2, 1)` (düz buton şekli)
3. **Add Component** → `ModernButtonController`
4. **Collider** → **Is Trigger** = ✅

**ModernButtonController Ayarları:**
- **Press Depth**: `0.1` (butonun ne kadar içeri gireceği)
- **Press Speed**: `5` (basma hızı)
- **Requires Player**: ☑️ (sadece player mi, yoksa pushable objeler de mi?)

**Action Ekleme:**
- **Actions** listesine yeni element ekle
- **Target Object**: Etkilenecek objeyi sürükle
- **Action Type**: Move/Scale/Activate seç
- **Move Direction**: Hareket yönü (örn: `(0, 5, 0)` yukarı)
- **Move Distance**: Hareket mesafesi
- **Reset On Release**: Buton bırakınca geri dönsün mü?

### 8.2 ModernLevelTrigger
**Level geçiş alanları oluşturmak için:**

1. **3D Object → Cube** → isim: `LevelExit`
2. Scale: `(3, 3, 1)` (kapı boyutu)
3. **Add Component** → `ModernLevelTrigger`
4. **Collider** → **Is Trigger** = ✅
5. **Mesh Renderer** → Disable (görünmez yapmak için)

**ModernLevelTrigger Ayarları:**
- **Next Level Name**: Sonraki level'in ismi (boş = otomatik)
- **Required Contact Time**: `0.5` (temas süresi)
- **Use Fade Effect**: ☑️ (geçiş efekti)
- **Completion Sound**: Level tamamlama sesi
- **Debug Mode**: Test sırasında ☑️

---

## 📋 ADIM 9: LAYER SETUP (OPSİYONEL)

### 9.1 Tag'ları Oluşturun
**Edit → Project Settings → Tags and Layers**:

**Tags sekmesinde:**
- ✅ `Player` (zaten var)
- **+** tıklayın → `Pushable` ekleyin

**Layers sekmesinde:**
- **Layer 8**: `PushableObjects`
- **Layer 9**: `Ground`

### 9.2 Tag ve Layer Atamaları

**Player objesine:**
- **Tag**: `Player` ✅

**Ground objelerine:**
- **Layer**: `Ground`

**Pushable nesnelere (kutular vb.):**
- **Tag**: `Pushable` ⭐ **ÖNEMLİ!**
- **Layer**: `PushableObjects`

---

## 📋 ADIM 9: CONTROLLER AYARLARI

### 9.1 EnhancedThirdPersonController Settings
**Player** objesinde ayarlayın:

**Karakter Hareket:**
- Move Speed: `2.0`
- Sprint Speed: `5.335`
- Rotation Smooth Time: `0.12`

**Zıplama ve Yerçekimi:**
- Jump Height: `1.2`
- Gravity: `-15.0`

**Zemin Kontrolü:**
- Grounded: ✅ (otomatik)
- Grounded Offset: `-0.14`
- Grounded Radius: `0.28` (CharacterController Radius ile aynı)
- Ground Layers: `-1` (Everything - varsayılan) veya istediğiniz layer'ları seçin

**Nesne Tutma Sistemi:**
- Grab Range: `2.0`
- Pushable Layer: `PushableObjects` seçin

### 9.2 Işınlanma Sistemi
**Clone Marker Prefab** oluşturun:
1. **Create Empty** → isim: `CloneMarker`
2. **Add Component → MeshRenderer** 
3. **Add Component → MeshFilter**
4. Mesh: `Sphere` seçin
5. Material: Parlak/Glowing material
6. Scale: `(0.2, 0.2, 0.2)`
7. **Prefab**'a çevirin

**Controller'da:**
- **Clone Marker Prefab**: Oluşturduğunuz prefab'ı sürükleyin

---

## 📋 ADIM 10: PUSHABLE OBJECTS

### 10.1 İtilebilir Kutu Oluştur
1. **3D Object → Cube** → isim: `PushableBox`
2. **Tag**: `Pushable` ⭐ **ZORUNLU!**
3. **Layer**: `PushableObjects`
4. **Add Component → Rigidbody**
5. **Add Component → PushableObject** script

### 10.2 PushableObject Script Ayarları
- Push Force: `5.0`
- Smooth Speed: `10.0`
- Hold Height: `1.0`

---

## 📋 ADIM 11: MOBILE UI (OPSİYONEL)

### 11.1 Canvas Oluştur
1. **UI → Canvas** → isim: `MobileUI`
2. **Canvas Scaler → UI Scale Mode**: `Scale With Screen Size`
3. **Reference Resolution**: `1920x1080`

### 11.2 Virtual Joystick
StarterAssets'ten mobile joystick prefab'ını kullanın:
- `UI_Canvas_StarterAssetsInputs_Joysticks`

### 11.3 Action Buttons
Canvas altında **UI → Button**'lar oluşturun:

**Clone Button:**
- Text: "CLONE (Q)"
- **OnClick()**: Player → StarterAssetsInputs → OnMobileClone

**Grab Button:**
- Text: "GRAB (E)"  
- **OnClick()**: Player → StarterAssetsInputs → OnMobileGrab

---

## 📋 ADIM 12: SES SİSTEMİ

### 12.1 Audio Clips
**Player** objesinde **Add Component → Audio Source**

### 12.2 Controller'da Ses Ayarları
**EnhancedThirdPersonController**:
- **Footstep Audio Clips**: Ayak sesi dosyalarını ekleyin
- **Landing Audio Clip**: İniş sesi ekleyin
- **Clone Create Sound**: Klon oluşturma sesi
- **Rewind Sound**: Işınlanma sesi

---

## 📋 ADIM 13: TEST VE OPTİMİZASYON

### 13.1 Test Senaryoları
✅ **Hareket**: WASD ile hareket  
✅ **Zıplama**: Space ile zıplama  
✅ **Koşma**: Shift + WASD ile koşma  
✅ **Klon**: Q ile klon oluştur, Q ile ışınlan  
✅ **Kutu**: E ile kutu tut/bırak  
✅ **Kamera**: Mouse ile kamera hareket  

### 13.2 Performans Ayarları
**Quality Settings**:
- **Mobile**: Low/Medium quality
- **PC**: Medium/High quality

### 13.3 Build Settings
**File → Build Settings**:
- **Mobile**: Android/iOS
- **PC**: Windows/Mac/Linux

---

## 🎮 KONTROLLER ÖZETİ

### PC Kontrolleri:
- **WASD**: Hareket
- **Mouse**: Kamera 
- **Space**: Zıplama
- **Shift**: Koşma
- **Q**: Klon oluştur / Işınlan
- **E**: Kutu tut / Bırak

### Mobile Kontrolleri:
- **Virtual Joystick**: Hareket
- **Touch Drag**: Kamera
- **Clone Button**: Klon/Işınlanma
- **Grab Button**: Kutu tutma

---

## 🚨 SORUN GİDERME

### ❌ Problem: Karakter hareket etmiyor
**Çözüm**: 
- Player Input component var mı?
- Input Actions doğru atanmış mı?
- StarterAssetsInputs script ekli mi?

### ❌ Problem: Karakter havada kalıyor
**Çözüm**:
- CharacterController Center'ı doğru mu? (genelde `(0, 1, 0)`)
- Ground Layers `-1` (Everything) olarak ayarlı mı?
- Grounded Radius, CharacterController Radius ile aynı mı?
- Zeminde Collider var mı?

### ❌ Problem: Kamera çalışmıyor  
**Çözüm**:
- CinemachineStyleCamera script ekli mi?
- Player objesinin "Player" tag'i var mı?
- Default Target manuel atanmış mı?

### ❌ Problem: Işınlanma çalışmıyor
**Çözüm**:
- PositionTracker script ekli mi?
- Clone Marker Prefab atanmış mı?

### ❌ Problem: Kutu tutma çalışmıyor
**Çözüm**:
- PushableObject script kutuda var mı?
- Pushable Layer doğru mu?
- Grab Point pozisyonu doğru mu?

### ❌ Problem: ModernButtonController çalışmıyor
**Çözüm**:
- Collider → Is Trigger ✅ mi?
- Action'larda Target Object atanmış mı?
- Player'da "Player" tag'i var mı?
- Pushable objede "Pushable" tag'i var mı? ⭐
- Requires Player ayarı doğru mu?

### ❌ Problem: "Tag: Pushable is not defined" hatası
**Çözüm**:
- Edit → Project Settings → Tags and Layers
- Tags sekmesi → + buton → "Pushable" ekle
- Pushable objelerinizin Tag'ini "Pushable" yap

### ❌ Problem: ModernLevelTrigger çalışmıyor
**Çözüm**:
- Collider → Is Trigger ✅ mi?
- Player'da "Player" tag'i var mı?
- Required Contact Time süresini beklediniz mi?
- Debug Mode'u açıp Console'u kontrol edin

---

## ✅ KURULUM TAMAMLANDI!

Bu adımları takip ettikten sonra:
- ✅ Modern character controller sistemi
- ✅ Işınlanma mekaniği
- ✅ Kutu tutma sistemi  
- ✅ Mobile uyumluluk
- ✅ Ses sistemi

**Artık oyununuz hem PC hem mobile'da mükemmel çalışacak!** 🎉 