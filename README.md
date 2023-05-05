# RAT

# Linux Vers Windows
1er message = infos sur le message
    1er caractère du message = instruction
        1: Lecture du Clipboard
        2: Réécriture texte du Clipboard
        3: Réécriture image du Clipboard
        4: Screenshot de l'écran
        5: Keylogger (envoi du fichier avec les keys)
        6: Photo webcam
        7: Le malware envoit son chemin
        8: Reverse shell
    A partir du 2ème caractère = taille du message

2ème message et + = contenu du message



# Windows vers Linux
1er message = infos sur le message
    1er caractère = type du message
        1: Texte
        2: Image

    A partir du 2ème caractère = taille du message

2ème message et + = contenu du message



Endodage: Base64 puis UTF8
Decodage: UTF8 puis Base64