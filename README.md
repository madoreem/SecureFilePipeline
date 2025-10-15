# Secure File Pipeline : Architecture Microservices en .NET

Ce projet est une démonstration d'une architecture microservices robuste et sécurisée pour le traitement de fichiers. Le système est conçu pour orchestrer un pipeline complet : réception de fichiers, analyse antivirus, extraction de métadonnées et stockage, le tout dans un environnement conteneurisé avec Docker.

## Architecture & Principes de Conception

Le projet met en œuvre une architecture découplée et résiliente, basée sur les principes suivants :

-   **Architecture Microservices** : Le système est divisé en services indépendants et spécialisés (`API`, `Analyse Antivirus`, `Extraction de Métadonnées`), chacun pouvant être développé, déployé et mis à l'échelle de manière autonome.
-   **Communication Asynchrone (Event-Driven)** : Les services communiquent de manière asynchrone via un système de fichiers partagés (volumes Docker). Un service dépose un fichier dans un répertoire, ce qui déclenche une action dans le service suivant. Ce modèle réduit le couplage et améliore la résilience du système.
-   **Sécurité Intégrée (Security by Design)** : La sécurité est une composante centrale du pipeline. Chaque fichier est systématiquement scanné par l'antivirus ClamAV avant tout traitement ultérieur.
-   **Extensibilité** : Le service de métadonnées utilise un *Patron Stratégie* pour permettre l'ajout facile de nouveaux extracteurs pour différents types de fichiers sans modifier le code existant.

## Flux de Traitement (Workflow)

1.  **Réception (`SecureFilePipeline.Api`)** : Un utilisateur envoie un fichier via une API REST. Le service valide la requête et dépose le fichier dans un volume partagé (`/app/uploads`).
2.  **Analyse Antivirus (`SecureFilePipeline.ClamAvService`)** : Ce service détecte l'arrivée du nouveau fichier, le scanne en streaming avec ClamAV.
    -   **Fichier sain** : Le fichier est déplacé vers le volume `/app/scanned`.
    -   **Fichier infecté** : Le fichier est mis en quarantaine dans le volume `/app/quarantine`.
3.  **Extraction de Métadonnées (`SecureFilePipeline.MetadataService`)** : Le service détecte les fichiers sains dans `/app/scanned`. Il sélectionne l'extracteur approprié (PDF, DOCX, image, etc.), traite le fichier, et sauvegarde les métadonnées extraites dans la base de données PostgreSQL.
4.  **Stockage et Consultation** : Les métadonnées sont stockées dans une table flexible utilisant un champ `jsonb` pour s'adapter aux différents types de données. L'API principale permet de consulter ces métadonnées.

## Caractéristiques Techniques

-   **Développement Backend en .NET 9** : Utilisation des dernières versions de .NET pour les services, y compris ASP.NET Core pour l'API et .NET Worker Services pour les tâches de fond.
-   **Base de Données PostgreSQL avec Entity Framework Core** : Modélisation des données et mapping avancé d'un `Dictionary<string, string>` vers un champ `jsonb` pour une flexibilité maximale.
-   **Conteneurisation avec Docker & Docker Compose** : Définition complète de l'environnement applicatif (services, base de données, antivirus) pour un déploiement simple et reproductible.
-   **Programmation Asynchrone et Multithreading** : Utilisation intensive de `async/await` et de `FileSystemWatcher` pour une gestion efficace des I/O et des événements.
-   **Principes SOLID et Design Patterns** : Application du *Strategy Pattern* pour les extracteurs de métadonnées, favorisant un code propre et maintenable.

## Technologies

| Domaine | Technologie |
| :--- | :--- |
| **Backend** | .NET 9, ASP.NET Core, .NET Worker Service |
| **Base de Données** | PostgreSQL, Entity Framework Core 9 |
| **Sécurité** | ClamAV |
| **Conteneurisation** | Docker, Docker Compose |
| **Librairies Notables** | PdfPig, DocumentFormat.OpenXml, MetadataExtractor |

## Déploiement et Exécution

Pour lancer le projet, assurez-vous que Docker et Docker Compose sont installés, puis exécutez la commande suivante à la racine du projet :

```bash
docker-compose up --build
```

L'API sera alors accessible à l'adresse `http://localhost:8080`.
