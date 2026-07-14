# AR-Indoor-Navigation

## Gliederung 
1. [Gruppenmitglieder](#1-gruppenmitglieder)
2. [Beschreibung und Motivation](#2-beschreibung-und-motivation)
3. [Zielfeatures](#3-zielfeatures)
4. [Aufgabenaufteilung](#4-aufgabenaufteilung)
5. [Verwendete Bibliotheken](#5-verwendete-bibliotheken)
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

## 4. Aufgabenaufteilung 
| Gruppenmitglied | Aufgaben |
|---|---|
|Dominic Tarnowski (598853) | Erstellung des initialen Unity-Projekts, Entwicklung Backendnavigation |
|Yahia Badr (574640) | Scannen von Etage und Mappen von Räumen, Layoutverbesserung Navigationslinie |
|Pauline Thiele (582014) | Entwicklung Visualisierung für Mobiltelefon, Verknüpfung von Front- und Backend, Dokumentation |
|Alle|Testen, Bugfixes|

## 5. Verwendete Bibliotheken
Im Code sind die verwendeten Bibliotheken in den jeweiligen Dateien mit Kommentaren gekennzeichnet. In der folgenden Tabelle ist eine Übersicht der genutzten Bibliotheken aufgeführt.

| Bibliothek | In welcher Datei verwendet | Quelle |
|---|---|---|
| `1` | bla |---| 


## 6. Ergebnisse

Die Anwendung wurde erfolgreich in Unity erstellt. Ein Einblick in dessen Funktionsweise bietet das beigefügte Video.

Nachfolgend wird auf die in Gliederungspunkt [3. Zielfeatures](#3-zielfeatures) genannten Features und deren Umsetzung eingegangen.

### 6.1 Aktuelle Standorterkennung 
Um die aktuelle Position des Nutzendens zu bestimmen, wurden insgesamt drei Ansätze verfolgt. Zuerst wurde probiert die Standortekennung über bereits [existierende Raumnummern](Assets/Core/tag.jpg), die von der Handykamera erfasst werden, zu implementieren. Dies scheiterte allerdings wegen schlechtem Kontrast zwischen Hintergrund und Schrift. Danach wurde versucht, die Standorterkennung über QR-Codes an den Etagenwänden umzusetzen. Auch dieser Ansatz scheiterte aus dem selben Grund. Wir denken, dass die Belichtung das Problem sein könnte. 
Um die genannten Lösungsansätze nachzuvollziehen, sind sie in [ins nichts führender Link]() enthalten und auskommentiert. 

Der dritte Ansatz und die aktuelle Umsetzung ist die Standorterkennung über die manuelle Auswahl des Startpunktes im Dropdown Menü. 

### 6.2 Routenoptimierung: Berechnung des kürzesten Weges
Die Berechnung des kürzesten Weges wurde mit Hilfe von ... umgesetzt.

### 6.3 Visuelle Wegweiser
Der visuelle Wegweiser ist in Form von grünen Pfeilen auf dem Handydisplay dargestellt. Sie zeigen die Richtung an, in die der Nutzende gehen muss, um zum Ziel zu gelangen. Die Pfeile werden dynamisch aktualisiert, basierend auf der  Position des Nutzenden und der berechneten Route. 

Am unteren Bildschirmrand wird die Entfernung zum Ziel in Metern angezeigt.
Wenn das Ziel erreicht ist, wird eine Meldung auf dem Display ausgegeben und die Navigation endet. 

### 6.4 Barrierefreiheit: Auswahl zwischen Fahrstuhl oder Treppe
Die Auswahlmöglichkeit wurde mit der Checkbox "Use Elevator", die sich neben der Auswahl von Start- und Endpunkt befindet, umgesetzt. Wenn diese angeklickt ist, wird die Information an das Backend weitergegeben. 

Da wir zum Ende des Projekts nur eine Etage gescannt und die Räume dazu gemappt haben, konnten wir die Navigation zwischen verschiedenen Etagen nicht umsetzten. Die visualisuelle Auswahlmöglichkeit ist jedoch bereits vorhanden. 

### 6.5 Plattformübergreifende Anwendung (iOS, Android)
Die Anwendung wurde bisher nur für Android Geräte umgesetzt. 

### 6.6 Point of Interests 
Es wurden Points of Interest (POIs), die in der ersten Etage existieren, wie zum Beispiel Drucker, Getränkeautomat und WCs hinzugefügt. Diese sind, wie alle anderen Räume, über das Start- und Enddropdown Menü auswählbar.

### 6.7 Dark- & Whitemode
Ein Dark- und Whitemode wurde in der Anwendung nicht implementiert, da diese fast ausschließlich aus dem Kamerabild besteht.

## 7. Fazit und Ausblick 
Im Rahmen des Projekts wurde eine funktionierende AR-Indoor-Navigation erstellt. Die Anwendung ist in der Lage, den Nutzenden zu einem ausgewählten Zielort innerhalb der ersten Etage des C-Gebäudes zu navigieren. 

Die Standorterkennung erfolgt aktuell über die manuelle Auswahl des Startpunktes im Dropdown Menü. Dies könnte in zukünftigen Projekten verbessert werden, indem eine Alternative zur Standorterkennung über QR-Codes oder Raumnummern gefunden wird. 

Wenn weitere Etagen gescannt und die Räume dazu gemappt werden, kann die Anwendung erweitert und die Funktionalität der Barrierefreiheit (Fahrstuhl oder Treppe) vollständig umgesetzt werden. 


## Todo 
- Bibs in Code kennzeichnen 
- QR Code 
- Link zu Lösungsansatz Standorterkennung 