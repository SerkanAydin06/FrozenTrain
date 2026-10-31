# FrozenTrain — kaplamasız PBR temel ve sonraki texture iş akışı

## Şu anki durum
Son üretim aşaması `FrozenTrainMinimalInteriorPass.Apply()` sonrasında trenin iç/dış mimarisindeki görünür yüzeyler **dokusuz, nötr URP/Lit malzemelerle** görüntülenir. Pencerelerdeki saydamlık korunur. Eski prosedürel doku dosyaları depoda kaynak olarak kalır, ancak oluşturulan sade trenin kaplamalarında kullanılmaz.

| Gelecekteki yüzey | Malzeme asset'i |
|---|---|
| Dış kaporta ve düz yüzeyler | `Assets/Art/Materials/TrainBaseline/01_Body.mat` |
| İç/dış duvarlar ve tavan | `Assets/Art/Materials/TrainBaseline/02_Walls.mat` |
| Tüm kat zeminleri | `Assets/Art/Materials/TrainBaseline/03_Floors.mat` |
| Merdiven, tekerlek, metal iskelet | `Assets/Art/Materials/TrainBaseline/04_Structure.mat` |
| Kabin, bölme ve geçiş kapıları | `Assets/Art/Materials/TrainBaseline/05_Doors.mat` |

Bu asset'ler ilk Unity yeniden üretiminde oluşturulur. **Var olan malzemeler otomatik sıfırlanmaz**; sonraki aşamada atanan PBR map'leri ve ayarlar korunur.

## Profesyonel texture aşaması
1. Yüzeylerin gerçek ölçülerini ve örtüşen geometrilerini doğrula; eş düzlemde çakışan yüzeyleri temizle. Kapı açıklıkları ve hareketli kanatları yeniden ölç.
2. Mevcut Unity primitive küplerin UV'leri varsayılan 0–1 yüz UV'sidir. 24 m uzunlukta aynı dokuyu esnetmek kabul edilemez. Mesh/UV unwrap veya dünya ölçekli triplanar shader kullan; **sabit texel density** sağla.
3. Her malzeme için **Base Color, Normal, Metallic/Smoothness, Ambient Occlusion** (gerekiyorsa Height) haritalarını ekle. Metal/Roughness paketini URP Lit mask kanallarına uygun dönüştür.
4. Dokular için MIP map, trilinear filtreleme, anizotropi ve doğru sRGB/normal import ayarlarını yap. Büyük yüzeyleri gerçek dünya ölçeğinde döşe.
5. Sonraki üretimlerde *bu beş slotun adını ve referansını değiştirme*; dokuları var olan slotlara uygula. Cam, far ve ışık için gerekiyorsa ayrı transparan/emissive set kullanılabilir.
6. Unity Play'de kapıları **kapalı, açık ve hareket ederken** görüntüle. Dar kapı kenarlarında Z-fighting, dışarı taşma ve yanlış collider durumlarını kontrol et.

Not: Bu aşama malzeme atama altyapısını hazırlıyor; UV unwrap, bake ve final yüksek çözünürlüklü PBR doku üretimi bu aşamaya dahil değildir.
