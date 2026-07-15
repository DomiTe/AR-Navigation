# AR-Indoor-Navigation

## Gliederung 
1. [Gruppenmitglieder](#1-gruppenmitglieder)
2. [Beschreibung und Motivation](#2-beschreibung-und-motivation)
3. [Zielfeatures](#3-zielfeatures)
4. [Eigenanteil und Aufgabenverteilung](#4-eigenanteil-und-aufgabenverteilung)
5. [Verwendete Assets](#5-verwendete-assets)
6. [Ergebnisse](#6-ergebnisse)
7. [Fazit und Ausblick](#7-fazit-und-ausblick)

## 1. Gruppenmitglieder
- Dominic Tarnowski (598853)
- Yahia Badr (574640)
- Pauline Thiele (582014)

## 2. Beschreibung und Motivation
Das Ziel ist eine mobile Anwendung zu erstellen, um die Raumsuche auf dem Unicampus zu vereinfachen. Gerade als neuer Studi an der HTW ist der Campus mit seinen vielen Gebäuden sehr unübersichtlich. Der existierende Campusplan ist nicht userfreundlich und nicht interaktiv. Auch innerhalb des Gebäudes (C Gebäude) gibt es nur einen Etagenplan pro Etage. Dieser ist nicht genau, da darauf keine einzelnen Räume abgebildet werden, sondern Teilgruppierungen von Raumnummern.

Um die Raumsuche dynamischer zu gestalten, möchten wir eine Unity Anwendung erstellen. Einstiegspunkte zur Anwendung sollen mit QR-Codes an den Fahrstühlen und Treppenaufgängen jeder Etage bereitgestellt werden. Nach dem Scannen des QR-Codes kann der Zielort per Dropdown Menü auswählt werden, woraufhin man mit visuellen Wegweisern auf dem Handy Display zum Ziel geführt wird.

## 3. Zielfeatures 
- Aktuelle Standorterkennung
- Routenoptimierung: Berechnung des kürzesten Weges
- Visuelle Wegweiser
- Barrierefreiheit: Auswahl zwischen Fahrstuhl oder Treppe
- Plattformübergreifende Anwendung (iOS, Android)
- Point of Interests (Labore, WCs, Getränkeautomaten, Drucker, …)
- Dark- & Whitemode

## 4. Eigenanteil und Aufgabenverteilung 
Der Eigenanteil der Gruppe in diesem Projekt ist die Erstellung von **Scans** (mit der Multiset App (IOs) erstellt und im Core/Scans/ Ordner enthalten), die Navigation mittels der **PathController.cs** und **SceneAlign.cs** Dateien, sowie die Visualisierung auf dem Mobiltelefon und die Verknüpfung von Front- und Backend mit der **InterfaceController.cs** Datei.

Wir haben KI genutzt um uns Hinweise fürs Debugging geben zu lassen, jedoch keine Inhalte von der KI generieren lassen. 

In der folgenden Tabelle ist die Aufgabenverteilung innerhalb der Gruppe dargestellt.

| Gruppenmitglied | Aufgaben | bearbeitete Dateien | 
|---|---|---| 
|Dominic Tarnowski (598853) | Erstellung des initialen Unity-Projekts, Implementierung Backendnavigation, Einfügen von POIs, Dokumentation | SceneAlign.cs, PathController.cs, InterfaceController.cs, Floor-Gameobject | 
|Yahia Badr (574640) | Scannen von Etage und Mappen von Räumen, Layoutverbesserung Navigationslinie | hallway_1st_floor_noMat_edited.glb, hallway_1st_floor_02_noMat_edited.glb, Floor-Gameobject, JsonExporter.cs, InterfaceController.cs | 
|Pauline Thiele (582014) | Entwicklung Visualisierung für Mobiltelefon, Verknüpfung von Front- und Backend, Dokumentation |InterfaceController.cs, UI-Gameobject, InterfaceManager-Gameobject | 
|Alle|Testen, Debugging || 

## 5. Verwendete Assets
In den folgenden Tabellen sind die genutzten Assets und Bibliotheken aufgeführt.

| Assets | Beschreibung | Quelle |
|---|---|---|
|`3D Scans`| im Scan Ordner | Multiset App (IOs) |

| Bibliothek | Beschreibung | Quelle |
|---|---|---|
|| |
|`AI Navigation`| für den Navigation Mesh | Unity Registry |
|`AR Foundation`| `ReferenceImageLibrary` für den Versuch den aktuellen Standort über QR-Codes zu ermitteln | Unity Registry |
|`Google ARCore XR Plugin`| Standart AR-Core Library | Unity Registry |
|`XR Interaction Toolkit`| Standart AR-Core Library | Unity Registry |
|`XR Plugin Management`| Standart AR-Core Library | Unity Registry |

## 6. Ergebnisse

Die Anwendung wurde erfolgreich in Unity erstellt. Ein Einblick in dessen Funktionsweise bietet das beigefügte Video.

Nachfolgend wird auf die in Gliederungspunkt [3. Zielfeatures](#3-zielfeatures) genannten Features und deren Umsetzung eingegangen.

### 6.1 Aktuelle Standorterkennung 
Um die aktuelle Position des Nutzendens zu bestimmen, wurden insgesamt drei Ansätze verfolgt. Zuerst wurde probiert die Standortekennung über bereits [existierende Raumnummern](Assets/Core/tag.jpg), die von der Handykamera erfasst werden, zu implementieren. Dies scheiterte allerdings wegen schlechtem Kontrast zwischen Hintergrund und Schrift. Danach wurde versucht, die Standorterkennung über [QR-Codes](Assets/Material/qr-code.jpg) (der QR-Code beinhaltet den Namen "Elevator 1") an den Etagenwänden umzusetzen. Auch dieser Ansatz scheiterte aus dem selben Grund. Wir denken, dass die Belichtung das Problem sein könnte. 
Um die genannten Lösungsansätze nachzuvollziehen, sind sie in [SceneAlign.cs](Assets/Core/SceneAlign.cs) enthalten und auskommentiert. 

Der dritte Ansatz und die aktuelle Umsetzung ist die Standorterkennung über die manuelle Auswahl des Startpunktes im Dropdown Menü. 

Die Navigation funktioniert nur dann, wenn die App in einer spezifischen Ausrichtung gestartet wurde. Grund dafür ist, dass AR-Foundation die Startrotation mit Öffnen der App zu Beginn festlegt. 

 #### **Aublick Kompass**

Eine Idee vom Professor nach der Präsentation war es, einen Kompass für die korrekte Ausrichtung der Szene beim Starten der Anwendung einzubauen. Das Problem ist hier allerdings: Auch wenn man einen Kompass einbauen könnte, würde die Anwendung initial nicht wissen, wo und in welcher Ausrichtung man aktuell zur gemappten Szene innerhalb der App steht. Das heißt, die größte Erweiterung für einen Ausblick wäre eine funktionierende Implementierung von QR-Code-Tracking (oder anderen Merkmalen).

### 6.2 Routenoptimierung: Berechnung des kürzesten Weges
Die Berechnung des kürzesten Weges wurde mit Hilfe von einem Navigation Mesh umgesetzt. Dieser Mesh wurde anhand von der Nachmodellierung des ersten Flurs (C Gebäude) mit der Library AI Navigation erstellt. Die Navigation erfolgt über die PathController.cs Datei, die die aktuelle Position des Nutzenden und den ausgewählten Zielort entgegennimmt und den kürzesten Weg berechnet. 

### 6.3 Visuelle Wegweiser
Der visuelle Wegweiser ist in Form von grünen Pfeilen auf dem Handydisplay dargestellt. Sie zeigen die Richtung an, in die der Nutzende gehen muss, um zum Ziel zu gelangen. Die Pfeile werden dynamisch aktualisiert, basierend auf der  Position des Nutzenden und der berechneten Route. 

Am unteren Bildschirmrand wird die Entfernung zum Ziel in Metern angezeigt.
Wenn das Ziel erreicht ist, wird eine Meldung auf dem Display ausgegeben und die Navigation endet (siehe Bild). 

### 6.4 Barrierefreiheit: Auswahl zwischen Fahrstuhl oder Treppe
Die Auswahlmöglichkeit wurde mit der Checkbox "Use Elevator", die sich neben der Auswahl von Start- und Endpunkt befindet, umgesetzt. Wenn diese angeklickt ist, wird die Information an das Backend weitergegeben. 

Da wir zum Ende des Projekts nur eine Etage gescannt und die Räume dazu gemappt haben, konnten wir die Navigation zwischen verschiedenen Etagen nicht umsetzten. Die visualisuelle Auswahlmöglichkeit ist jedoch bereits vorhanden. 

### 6.5 Plattformübergreifende Anwendung (iOS, Android)
Die Anwendung wurde bisher nur für Android Geräte getestet. 

### 6.6 Point of Interests 
Es wurden Points of Interest (POIs), die in der ersten Etage existieren, wie zum Beispiel Drucker, Getränkeautomat und WCs hinzugefügt. Diese sind, wie alle anderen Räume, über das Start- und Enddropdown Menü auswählbar.

### 6.7 Dark- & Whitemode
Das Umschalten zwischen Dark- und Whitemode wurde in der Anwendung nicht implementiert. Während der Projektumsetzung wurde über die Sinnhaftigkeit dieses Features innerhalb der Projektgruppe gesprochen und festgelegt, dass es nicht notwendig ist, da die Anwendung fast ausschließlich aus dem Kamerabild besteht.

## 7. Fazit und Ausblick 
Im Rahmen des Projekts wurde ein funktionierender AR-Indoor-Navigation-Prototyp erstellt. Die Anwendung ist in der Lage, den Nutzenden von einem ausgewählten Start- zu  einem selbstgewählten Zielort innerhalb der ersten Etage des C-Gebäudes zu navigieren. 

Die Standorterkennung erfolgt aktuell über die manuelle Auswahl des Startpunktes im Dropdown Menü. Dies könnte in zukünftigen Projekten verbessert werden, indem eine Alternative zur Standorterkennung über QR-Codes oder Raumnummern gefunden wird. 

Wenn weitere Etagen gescannt und die Räume dazu gemappt werden, kann die Anwendung erweitert und die Funktionalität der Barrierefreiheit (Fahrstuhl oder Treppe) vollständig umgesetzt werden. 


## Todo 
- QR Code einfügen als png & Lösungsansatz im Code einfügen ud auskommentieren (Link in Doku)
- Kompass 