# FrozenTrain — Proje Hafızası

## Proje özeti
- Unity 6.3 LTS (6000.3.25f1), URP, Windows hedefli birinci şahıs hayatta kalma/yönetim prototipi.
- Ana sahne: `Assets/Scenes/FPSPrototype.unity`.
- Beş kullanılabilir vagon (Lokomotif, Jeneratör, Yaşam, Atölye, Sera), üç kat; vagon başına 24 × 9.2 m.
- Dört ayrı 4 × 9.2 m vestibülde merdivenler; normal merdivenler vagonların içinde değil.
- Trenin sahne/prefab üretim giriş noktası: `Frozen Train > Rebuild Authored Train`.

## Önceki bağlam / dikkat noktaları
- 2026-10-30 depo kodu incelendi; Unity Editor çalıştırılmadı, ekran görüntüleri piksel düzeyinde değerlendirilmedi.
- Zafer koşulu/mesafe hedefi tanımlı değil; kullanıcı istemeden yeni hikâye veya hedef tasarlama.
- GitHub Actions screenshot paketleme işinin son 2026-10-05 çalıştırması başarısız/iptal durumunda. Unity derlemesi yerine geçmez.

## Kalıcı çalışma protokolü
- Kaynak: Bu deponun güncel kodu ve belgeleri. Sohbet hafızası eksik veya eski olabilir.
- GitHub bağlantısını kontrol et, mevcut GitHub araçlarıyla doğrudan çalış.
- Önce `AGENTS.md` (ChatGPT/Codex) veya `CLAUDE.md` (Claude) kısa rehberi ve yalnızca ilgili 1–3 kaynak dosya/aralık oku. Bu uzun hafızayı her seferinde baştan sona okumak yerine ihtiyaç duyulan başlık/son karar bölümünü aç.
- Repo kuralları ve beş vagonlu mimari korunmalı; kapsam dışı hikâye/NPC veya yeni vagon ekleme.
- Mümkün olduğunda GitHub'a doğrudan tek tutarlı değişiklik yap; commit bağlantısını bildir.
- Bu arşive yalnızca **kalıcı karar, mimari değişiklik, önemli bulgu veya çözülmemiş engel** ekle; sıradan commit ve tekrarları kaydetme.
- Yanıt dili Türkçe; kısa, net, gereksiz anlatımdan uzak.
- Eski kararları uygulanmış varsayma; güncel kod ile doğrula.

## Çalışma günlüğü
- 2026-10-30: Proje devamlılığı için başlangıç dosyası oluşturuldu (`eb64ed3`).
- 2026-10-30: `PlayerInteraction.cs` hedef seçiminde gerçek collider için görüş hattı denetimi eklendi; duvar arkasından etkileşim riski azaltıldı. `TrainScreenshotCapture.cs` normal Play'de kapalı, `Tools > FrozenTrain > Capture Screenshots on Play` üzerinden isteğe bağlı açılıyor. `TrainPrototypeState.cs` sera ve atölye kaynak akışları güç verimliliğiyle eşlendi. Commit: `8ca785d4206f66053a971005740a6eda53490986`.
- 2026-10-30: `TrainManagementPanel.cs` tam panel scroll ve dar ekran için satırlara bölünen göstergeler aldı. Commit: `6fdc790d7b2dd8aa5ed4730d6b9b841bc23980e1`.
- 2026-10-30: Tek slot manuel kayıt/yükleme (F5/F9, panel ve ölüm ekranından F9) dinamik oyun durumu ve oyuncu konumunu `PlayerPrefs` ile saklıyor. `TrainPrototypeState.cs` ve `TrainManagementPanel.cs`. Commit: `a9dcfc34c7b7915afab0c8721c0f0bccf7e851e8`.
- Kaynak dosyaların temel statik parantez/alan kontrolleri yapıldı. Unity Editor derlemesi, Play Mode, fiziksel geçiş ve farklı çözünürlük testleri henüz çalıştırılmadı.
- Bekleyen: Unity 6000.3.25f1'de Play Mode QA; görüş hattı (cam/kapı/duvar), dar pencere UI, kayıt/yükleme ve güç verimi testleri. Ekran görüntüsü CI iptal sebebi GitHub run loglarından teyit edilmeli. Kazanma hedefi kullanıcı kararıyla belirlenmeli.


## 2026-10-30 — Havada kalan dekor / montaj düzeltmeleri
- Depodaki `Docs/Screenshots/` görsellerinden vagonların üç katı, yatak kabinleri, geçiş merdivenleri ve teknik bölümler örneklenerek incelendi. Bunlar 2026-10-05 kaynak görüntüleridir, yeni Play Mode doğrulaması değildir.
- `Assets/Editor/FrozenTrainProductionArtPro.cs`: Yatak kabini `SleepPod` grubu sıfır orijinli, gerçek yatakları `BedFrame.localPosition` ile ofsetli olduğu için `__ProductionSleepPod` dekorları yanlışlıkla vagon merkezine ve alt kata yerleşiyordu. Dekor grubu gerçek yatak konumuna taşındı, her iki koridor tarafına bakan yataklar yönlendirildi; korkuluk, okuma ışığı, kabin rafı ve ana raflara taşıyıcı parçalar eklendi. Sera sulama üst borularına tavan askıları yerleştirildi.
- `Assets/Editor/FrozenTrainLongVestibuleArchitecture.cs`, `FrozenTrainLongLightingPass.cs`, `FrozenTrainPost24ScreenshotPolish.cs`, `FrozenTrainProductionArtPro.cs`: Aydınlatma/menfez panellerinin tavan yüzeyinden ayrı durmasına yol açan ofsetler düzeltildi. Koridor işaretlerinin mevcut duvar yüzeyi hizası korundu.
- İlgili commitler: `eb739cb567e210eb1c83e352120f53b4b258e01f`, `205111abcfce99807ce0f24712614fbdb02c09cb`, `fb72d391fa1fe361d4719a06ef5b6c167167def4`.
- **Zorunlu Unity adımı:** Değişen kodu Unity'ye aldıktan sonra `Frozen Train > Rebuild Authored Train` komutunu bir kez çalıştırıp sahneyi kaydet. Önceden üretilmiş prefab/sahne varlıkları kod değişikliğiyle kendi kendine yeniden hesaplanmaz.
- Statik kaynak kontrolü yapıldı; bu ortamda Unity Editor derlemesi, yeniden üretilmiş prefabların görsel teyidi ve Play Mode testleri çalıştırılmadı. Yeniden üretim sonrası aynı ekran görüntülerinin güncellenmesi ve boşta kalan başka parçaların kontrolü bekliyor.

## 2026-10-31 — Temel vagon iskeletine dönüş
- Kullanıcı yeni temel olarak yalnızca lokomotif ve dört vagonun mimarisini, kapıları ve vagonlar arası merdivenleri istedi. İç donanımın tamamı aşamalı yeniden tasarım için kaldırıldı.
- `Assets/Editor/FrozenTrainMinimalInteriorPass.cs` ve `.meta` eklendi; `Assets/Editor/FrozenTrainUnifiedAuthoring.cs` üretim zincirinin **sonuna** temizlik aşaması eklendi. Her yeni `Frozen Train > Rebuild Authored Train` çalıştırmasında kalıcı hale gelen prefab çıktısı yeniden sadeleştirilecek.
- Kalanlar: beş 24 m vagon gövdesi ve dış kaplamaları, zemin/çatı/duvar/pencere açıklıkları, üst kat kabin bölmeleri ve kapıları, ana koridorun işlevsel bölme kapısı, dört 4 m geçişin kapıları, merdiven basamakları/korkulukları/taşıyıcıları, yalnızca görünmez asgari aydınlatma. Dış çevreye müdahale edilmedi.
- Kaldırılanlar: lokomotif konsol/mobilyaları, jeneratör/atölye/sera ve yaşam vagonu donanımları, kabin yatakları, raflar, paneller, dekoratif tavan/floor parçaları, ekranlar ve diğer QA/prod. süslemeleri. Nesneler üretilen prefabda siliniyor; eski kaynak üretim kodları şimdilik saklandı.
- Kaldırılan fiziksel `ManagementTerminal` artık E ile açılmayacak; oyun yönetim panelinin klavye kısayolu kullanılabilir. Mevcut yaşam simülasyonu kaynakları arka planda çalışmaya devam eder.
- Commit: `2e2037a983df7c5cef990c3916f75f97dd9e2ced`.
- **Unity adımı:** Yeni kodu çekip `Frozen Train > Rebuild Authored Train` çalıştır, `Ctrl+S` ile kaydet. Mevcut sahne/prefab bu adım yapılmadan sadeleşmez.
- Kod düzeyinde temel statik kontrol yapıldı; gerçek Unity derlemesi ve yeni sahnenin görsel/fizik testleri bu ortamda yapılmadı.

## 2026-10-31 — Turuncu kapı dekorlarının silinmesi ve dik tırmanma merdiveni
- Kullanıcı mevcut sade sahneden kapı üstü süslerini, turuncu bantları, işaretleri ve diğer kapı dekorlarını çıkartmayı; tüm basamaklı merdivenlerin düz dik tırmanma merdivenine dönüştürülmesini istedi.
- `Assets/Editor/FrozenTrainLongVestibuleArchitecture.cs`: 4 adet vestibülde `BuildStairCore` yerine `BuildLadderCore`; her vestibülde ana↔üst ve teknik↔ana için birer `UpperLadder`/`LowerLadder` toplam 8 dik raylı-basamaklı tırmanma merdiveni, `DeckLadder` ile E etkileşimi. Basamaklı eski merdivenler kaldırıldı; uzun merdiven boşlukları yerine 1.08×1.10 m kompakt açıklıklar ve sağlam zemin üzerindeki çıkış noktaları tanımlandı. Konstrüksiyonun geçiş kapıları korunuyor; turuncu `DoorBand`, zemin işaretleri kaldırıldı.
- `Assets/Editor/FrozenTrainMinimalInteriorPass.cs`: yalnızca `DoorPanel` bileşenleri tutularak kapı üzerindeki portakal rengi şeritler, işaretler, başlık dekorları/ek kulplar temizleniyor; kabin kapılarının cephe süsleri yerine basit açık kapı yaprakları kalıyor. Konnektör beyaz listesi dik merdivenlere geçti; artık basamak/korkuluk saklamıyor.
- `Assets/Scripts/Interaction/DeckLadder.cs`: etkileşim aktarıcılarının vestibülün tüm duvar/kapılarına yayılması önlendi; sadece ilgili merdivenin çarpışma bileşenlerine bağlanıyor.
- `Assets/Editor/FrozenTrainUnifiedAuthoring.cs`: eski basamak/stringer son geçişi devreden çıkarıldı. Kaynak kodu denetimi yapıldı; Unity Editor Play Mode testi bu ortamda çalıştırılamadı.
- Commit: `6281b6e35900fe17f4c1982fd6bf29a72cf1dd0e`.
- **Unity'de uygulama:** Güncel `main` kodunu çek → `Frozen Train > Rebuild Authored Train` → sahneyi kaydet (`Ctrl+S`) → E ile alt/üst kat merdivenlerini test et.

## 2026-10-31 — Minimal dış tasarım ve duvara sabit dik merdivenler
- Kullanıcı trenin dışındaki gereksiz/söküntü parçalarının, içeri taşan detayların temizlenmesini; profesyonel ama sade dış gövde ve ayrı merdiven odası olmadan duvara dayanan dik merdiven istedi.
- `Assets/Editor/FrozenTrainMinimalInteriorPass.cs`: `ExteriorDressing`, `__LongExteriorV1` ve eski üretim-art dış detayları son temizleme geçişinde siliniyor. Yapısal `Shell` (24 m gövde, üç kat, pencereler, çatı, alt şasi) korunuyor. `__MinimalExteriorV1` yalnızca koyu dış alt eşik, her boginin iki dingiline oturan tekerlek+göbek ve lokomotifin önünde kalan sade burun/iki beyaz far ekliyor. Çıkarılanlar: eski turuncu şeritler, saçaklar, kar birikintileri/buzlar, fazla kapaklar/çift tekerlek setleri, öne/yanlara taşan süsler; geçiş çatı kar kaplaması da çıkarıldı. Kaynak modeller silinmedi, rebuild sonucunda bulunmazlar.
- `Assets/Editor/FrozenTrainLongVestibuleArchitecture.cs`: `BuildMainTransit` artık iç merdiven odası, bölme duvarı, kapı ve üst başlık oluşturmuyor; yalnızca mevcut ana geçiş kapılarını kuruyor. 4 vestibülün toplam 8 dik merdiveni -Z dış yan duvarın içine sabitlenmiş küçük bağlantı kollarıyla yerleştirildi. Kat delikleri yan duvar kenarına çekildi; çıkış noktaları sağlam zemin üzerinde tutuldu. Tutamaçlar üst kat zemininden yukarı uzanıyor. Merdiven E etkileşimi korunuyor.
- `Assets/Editor/FrozenTrainUnifiedAuthoring.cs`: Eski kalın dış trim geçişi kaldırıldı, sade dış tasarımı uygulayan son adım tek otorite oldu.
- Commit: `3ff0fca84a9c7cf4f3323b5610cc964058af3477` (+ çatıda son sadeleştirme ve dokümantasyon commit'i).
- Doğrulama: değiştirilmiş kodda statik parantez/simge kontrolleri ve varlık malzeme yolları denetlendi. **Unity Editor çalıştırılıp prefablar yeniden üretilmedi**. Yeni sahneyi görmek için GitHub'dan çek, `Frozen Train > Rebuild Authored Train`, ardından `Ctrl+S`; merdivenler, kapılar ve dış cepheyi Play Mode'da kontrol et.

## 2026-10-31 — Merdiven oranları ve kaplama titreme/aliasing düzeltmesi
- Kullanıcı duvara dayalı merdivenlerin çok uzun göründüğünü ve kaplamalarda hareket esnasında titreme yaşandığını bildirdi; yüksek kalite PC görünümü öncelikli.
- Depodaki en yeni `Docs/Screenshots` seti **2026-10-30 23:07:41** tarihli. `gecis1_merdiven.jpg` hâlâ eski basamaklı tasarımı gösteriyor; mevcut düz dik merdivenlerin son görünümü bu görüntülerden doğrulanamaz. `vagon2_kat2.jpg` gibi karelerde zemin ve tavanda yoğun ince desenler var.
- `FrozenTrainLongVestibuleArchitecture.cs`: duvara asılı dik merdivenlerde korkuluğun üst kat zeminini 1.08 m aşması azaltıldı (0.14 m), ray/basamak genişlikleri inceltildi; toplam erişim yüksekliği ve `E` ile kat geçişi korundu.
- `FrozenTrainProductionArtPro.cs` ve mevcut `FT_PRO_Floor.mat`/`FT_PRO_Wall.mat`: yüksek frekanslı zemin doku tekrarı 3.5→1.25, duvar 2→1; normal etkisi düşürüldü, metaliklik dengelendi. Kaynak kodun her rebuild'de değerleri eski haline çevirmemesi için üretim otoritesi de güncellendi.
- `Assets/Art/Textures/Generated/TrainProduction/` altındaki dört zemin/duvar albedo ve normal `.meta`: trilinear+mipmap, zemin 8× ve duvar 4× anizotropi, lossless import ayarları. `EnsureTexture` da aynı ayarları uyguluyor.
- `PC_RPAsset.asset`: MSAA 1→4. `FrozenTrainBuilder.cs`: mevcut/yeniden oluşturulan oyuncu kamerasında SMAA High ve MSAA, near clip 0.04→0.10, far clip 260→220. Render dokümanlarına göre MSAA polygon kenarlarını, trilinear+mipmap/anisotropic filtreleme uzak/oblik doku taramasını azaltır; üst üste binen tam eşdüzlemsel geometriyi tek başına gidermez.
- Commit: `18a8521c39263db7bcec0859a09a2d14613b4544`.
- **Unity kontrolü hâlâ gerekli:** `Frozen Train > Rebuild Authored Train` → `Ctrl+S`; özellikle geçişlerde merdiven uzunluğu, zemin/duvar titreşimi, ve hareket esnasındaki aliasing'i Play Mode'da kontrol et. Yeni ekran görüntülerini tarihle kaydet. Bu ortamda Unity derlemesi/render doğrulaması yapılmadı.

## 2026-10-31 — Varsayılan açık, adaptif QA ekran görüntüleri
- `Assets/Scripts/DevTools/TrainScreenshotCapture.cs` yeniden düzenlendi: `Tools > FrozenTrain > Capture Screenshots on Play` yeni `EditorPrefs` V2 anahtarıyla varsayılan AÇIK; kullanıcı isterse menüden kapatabilir. Sadece `FPSPrototype` Editor Play modunda devreye girer.
- Önceki sabit kamera pozisyonları kaldırıldı. Kamera planı sahnedeki gerçek tren/vagon gövde render bounds, fiziksel zemin isimleri, vagon geçişleri, işlevsel `UpperLadder`/`LowerLadder` ve örnek kapılardan üretilir. Fiziksel olarak duvara gömülen kamera noktaları alternatif konumlarla denenir; bulunamazsa görüntü atlanır. Bu, harici yapay zekâ model çağrısı değil, geometriye duyarlı algoritmik seçimdir.
- FPS oyuncu kamerasını oynatmayan ayrı `Camera`, otomatik 1600×900 JPEG kalite 92; bir çalışmada en fazla 60 fotoğraf. Oyuncu kontrolleri kapatılmaz. Eski JPG/PNG her çalıştırma başında silinir, `Docs/Screenshots/README.md` son çekimin tarihini ve görüntü noktalarını içerir. Tarihli arşiv oluşturulmaz.
- Değişiklik commit'i: `6d755be01c5186007419347c5e86bbbbdb4467cb` (+ bu V2 ayarı ve dokümantasyon güncellemesi).
- **Sınır:** Görseller Play sırasında yerel klasörde oluşur; otomatik GitHub'a yüklenmez. Asistanın uzaktan görebilmesi için sadece en güncel `Docs/Screenshots/` dosyaları commit/push edilmelidir. Git geçmişindeki eski commitlerin görüntü sürümleri kalabilir; yalnızca güncel klasör tek set tutar.
- Kaynak statik kontrolü yapıldı; gerçek Unity Editor Play Mode veya kamera render doğrulaması bu oturumda çalıştırılamadı.

## 2026-10-31 — Kaplamasız URP/PBR temel ve kapı hizası
- Kullanıcı şu an hiçbir desenli kaplama istemiyor; sonraki aşamada kaliteli texture/PBR setleri eklenecek. Ayrıca kapılar açıklığa tam oturmalı, duvardan taşmamalı ve kapanmalı.
- `FrozenTrainMinimalInteriorPass.cs`: üretim son adımında görünür zemin, duvar, gövde, metal konstrüksiyon ve kapılardaki eski desenli malzeme referanslarını temizleyip yeni **dokunsuz** `Assets/Art/Materials/TrainBaseline/01_Body.mat`, `02_Walls.mat`, `03_Floors.mat`, `04_Structure.mat`, `05_Doors.mat` URP/Lit malzeme slotlarına atıyor. Mevcut cam malzemeleri saydamlığı kaybetmesin diye korunuyor. Bu slotlar ilk rebuild'de üretilir; sonradan elle konan PBR haritaları rebuild'de silinmez. Eski texture dosyaları güvenli geri alma için kaynakta durur, sade prefab materyallerinde kullanılmaz.
- `FrozenTrainLongVestibuleArchitecture.cs`: alt/ana/üst geçiş kapılarının merkez, yükseklik ve genişliği gerçek `L.*EndOpening` ve kat üst yüzeyinden türetilir; panelin tüm çevresinde yaklaşık 1 cm montaj payı. Açılma ekseni duvar boyunca kalır ve gövde dışına uzanmaz.
- `FrozenTrainMainStreetArchitecture.cs`: işlev vagonu bölme kapısı duvar merkezine alındı; panel ve etkileşim kolideri yaklaşık 1cm kenar boşluğu verecek şekilde eşleştirildi.
- `FrozenTrainMinimalInteriorPass.cs`: dekoratif, sürekli açık `PocketDoorLeaf` parçaları silindi; bunun yerine üst kabinlerde 10 adet/vagon gerçek `SlidingDoor` ve geometrik açıklıklarına eşleşen kapı panelleri bulunur.
- Önemli sahne üretim hatası düzeltmesi: `FrozenTrainBuilder.Build` önce prefablardan sahne nesnelerini oluşturuyor, son tasarım adımları sonradan prefab assetlerini değiştiriyor. Son adım `RefreshAuthoredSceneInstances()` ile mevcut sahne vagon/geçiş örneklerini güncel prefablarla değiştiriyor ve sahneyi kaydediyor. Önceki sahne prefabları bu yüzden eski çizimler gösterebiliyordu.
- Kaynak kod parantez/statik kontrolü yapıldı, **Unity derlemesi, tam kapı kapanışı ve sahne görsel testi burada yapılamadı**. `Frozen Train > Rebuild Authored Train` komutunu çalıştırıp Play modunda geçiş/kabin kapılarını açıp kapatarak kontrol et.
- Pro texture iş akışı: `Docs/TEXTURE_WORKFLOW.md`.

## 2026-10-31 — Eski editör geçişlerinin temizliği
- Son temizlik geçişinin (`FrozenTrainMinimalInteriorPass`) sildiği çıktıları üreten ölü geçişler kaldırıldı: `ScreenshotQASafety`, `LongLightingPass`, `Post24ScreenshotPolish`, `ProductionArt` sarmalayıcısı; hiç çağrılmayan `CirculationFinalizer`, `FinalScreenshotFix`, `LongExteriorPass`, `UpperDeckCirculation`, `UpperDeckFinalPolish`.
- `ScreenshotQARefinement` → `FrozenTrainExteriorScenery` (aynı GUID): sadece sahnedeki dağ/kaya/kar/ray ölçüsü kısmı kaldı; `ExteriorMaterialFix` buraya birleştirildi (FT_ENV_Snow/Rock doğrudan atanıyor).
- Zincir: Builder → ProductionArtPro → ExteriorScenery → MainStreet* → LongVestibule → MinimalInterior. Unity'de derleme/rebuild bu ortamda yapılamadı.
