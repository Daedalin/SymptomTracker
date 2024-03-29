# SymptomTracker

## Was kann die App
In dem SymptomTracker könnt ihr euer Essen, Symtohm, Stress und Stimmung eintragen. 
Beim eintragen gibt es eine Schnellauswahl die aus den bisher eingetragenen Titeln besteht. 
(Bild)

Außerdem kann man sich alle Ereignisse von bestimmten Tagen anzeigen lassen. 

### Was noch kommten soll 
Bald kann man auch sich Reports erstellen. Welcher in Form einer PDF alle Ereignisse angezeigt, die bestimmten Filter Kriterien erfüllen. 

### Features für Premium Nutzer
Man kann an Ereignissen Bilder hochladen welche auch später im report eigezeigt werden. 

## Wie bekomme ich die App
Ihr könnt euch die APK aus den Release in github herunterladen. 
Oder ihr meldet euch bei Firebase App Distributionen an. Dort bekommt ihr eine Email wenn eine neue Version veröffentlicht wird. Desweiteren könnt ihr auch ganz bequem die neue Version mit der "App Tester" App herunterladen und installier. 
[Firebase Appdistribution](https://appdistribution.firebase.dev/i/f8d2ec0b8a6204a1 )

## Datenschutz 
Die Daten wie auch die Bilder werden in Firebase in der Echtzeitdatenbank und im Filestore gespeichert. Firebase ist ein Google Dienst bei dem ich die Server in Frankfurt ausgewählt hab. Alle Daten werden auf dem Handy mit eime Key, der in den Einstellungen steht, verschlüsselt und natürlich auch entschlüsselt. 
Der Key wird nur auf dem Handy gespeichert. Und sollte auch vom Nutzer selbst gesichert werden, da er auch beim Update neu eingetragen werden muss. Bei Verlust des Key können die Dante nich mehr abgerufen werden. 

### Transparenz
Ich kann die login Email Adressen sehen und mit dieser auch sehen an welchen Tagen etwas eingetragen wurde. Anhand der Länge der verschlüsselten Daten kann man auch grob abschätzen wie viel eingetragen wurde. Genauso die Bilder werden mit dem Datum gespeichert aber auch diese sind natürlich verschlüsselt.
(Bild) 



