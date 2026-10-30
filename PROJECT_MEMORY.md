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
- Kod değiştirmeden önce `PROJECT_MEMORY.md`, `CLAUDE.md` ve ilgili kaynak dosyaları incele.
- Repo kuralları ve beş vagonlu mimari korunmalı; kapsam dışı hikâye/NPC veya yeni vagon ekleme.
- İstenen değişiklikleri mümkün olduğunda doğrudan GitHub'a uygula; commit bağlantısını bildir.
- Her görevden sonra yeni karar, değişen dosyalar, doğrulama ve bekleyen işleri bu dosyaya yaz.
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
