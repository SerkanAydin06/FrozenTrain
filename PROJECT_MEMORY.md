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
