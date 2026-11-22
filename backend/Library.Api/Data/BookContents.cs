using Microsoft.EntityFrameworkCore;

namespace Library.Api.Data;

public static class BookContents
{
    public static async Task ApplyAsync(AppDbContext db)
    {
        var books = await db.Books.ToListAsync();
        if (books.Count == 0) return;

        foreach (var b in books)
            b.File = GetRealBookText(b.Title, b.Author, b.Description);

        await db.SaveChangesAsync();
    }

    private static string GetRealBookText(string title, string author, string desc) => title switch
    {
        "L'ADN raconté" =>
            "Chapitre I — Le fil invisible de deux mètres\n" +
            "Au cœur de presque chacune des trente mille milliards de cellules qui composent le corps humain se cache une molécule fascinante : l'acide désoxyribonucléique, plus connu sous le sigle ADN. Si l'on pouvait dérouler bout à bout le filament d'ADN contenu dans le noyau d'une seule cellule microscopique, il mesurerait près de deux mètres de long. Pour tenir dans un espace mille fois plus petit qu'un grain de sable, cette molécule s'enroule autour de protéines appelées histones, formant une structure ultra-compacte.\n\n" +
            "En 1953, les clichés de diffraction aux rayons X réalisés par Rosalind Franklin permettent à James Watson et Francis Crick de révéler son architecture en double hélice. Cette découverte majeure éclaire enfin le mécanisme de l'hérédité : la façon dont chaque être vivant transmet ses caractères à sa descendance.\n\n" +
            "Chapitre II — Un alphabet universel à quatre lettres\n" +
            "Qu'il s'agisse d'une bactérie marine, d'un cèdre de l'Atlas, d'une abeille ou d'un être humain, tout le monde vivant parle exactement la même langue chimique. Cet alphabet ne compte que quatre bases azotées : l'Adénine (A), la Thymine (T), la Cytosine (C) et la Guanine (G). Sur les deux brins de l'hélice, ces lettres s'assemblent toujours selon une règle stricte : A s'apparie avec T, et C avec G.\n\n" +
            "Grâce à cette complémentarité, lorsqu'une cellule se divise, les deux brins se séparent comme une fermeture éclair et chaque moitié sert de modèle exact pour reconstruire son brin partenaire. Des enzymes spécialisées relisent ensuite la copie pour corriger la moindre erreur.\n\n" +
            "Chapitre III — Des gènes aux protéines\n" +
            "Un gène est un segment précis de cette longue séquence d'ADN qui contient la recette de fabrication d'une protéine. Pour lire cette recette sans abîmer l'original conservé dans le noyau, la cellule fabrique une copie temporaire : l'ARN messager. Ce messager rejoint ensuite les ribosomes, véritables ateliers d'assemblage de la cellule, qui traduisent chaque triplet de lettres en un acide aminé précis.\n\n" +
            "Les protéines ainsi produites assurent toutes les fonctions vitales : l'hémoglobine transporte l'oxygène dans le sang, les anticorps défendent l'organisme contre les virus et les enzymes assurent la digestion.\n\n" +
            "Chapitre IV — La révolution génomique et l'éthique\n" +
            "En 2003, le premier séquençage complet du génome humain a nécessité treize années de travail international et des milliards de dollars. Aujourd'hui, cette opération s'effectue en quelques heures. Elle permet de diagnostiquer précocement des maladies rares et d'adapter les traitements médicaux au profil génétique de chaque patient. Mais ce pouvoir inédit impose aussi des règles éthiques strictes pour protéger la confidentialité des données génétiques et préserver la dignité humaine.",

        "Introduction à l'IA" =>
            "Chapitre I — De la programmation classique à l'apprentissage\n" +
            "Pendant plusieurs décennies, l'informatique traditionnelle a reposé sur un principe simple : l'humain devait écrire à la main toutes les règles logiques que la machine devait exécuter (« si telle condition est vraie, alors exécuter telle instruction »). Si cette approche fonctionne parfaitement pour calculer une facture ou trier une base de données, elle échoue dès qu'il s'agit de reconnaître un visage sur une photo floue ou de comprendre l'ironie d'une phrase.\n\n" +
            "L'apprentissage automatique (Machine Learning) renverse complètement ce paradigme. Au lieu de dicter les règles pas à pas, on fournit à un algorithme des milliers d'exemples annotés afin qu'il découvre lui-même les régularités statistiques cachées dans les données.\n\n" +
            "Chapitre II — Réseaux de neurones artificiels\n" +
            "Inspirés librement des neurones biologiques du cerveau, les réseaux de neurones profonds (Deep Learning) empilent des dizaines de couches de calculs mathématiques. Chaque neurone artificiel reçoit des signaux numériques, leur applique un poids ajustable, et transmet un résultat à la couche suivante.\n\n" +
            "Lors de la phase d'entraînement, le modèle fait des prédictions sur des millions d'exemples, mesure son erreur grâce à une fonction de coût, puis ajuste progressivement ses paramètres internes par rétropropagation du gradient jusqu'à atteindre une grande précision.\n\n" +
            "Chapitre III — L'architecture Transformer et les grands modèles\n" +
            "En 2017, l'invention de l'architecture Transformer et du mécanisme d'attention a bouleversé le traitement du langage naturel. Contrairement aux anciens modèles qui lisaient un texte mot après mot, un Transformer analyse simultanément les relations entre tous les mots d'un paragraphe, saisissant ainsi le contexte global, les nuances et les références lointaines.\n\n" +
            "C'est cette technologie qui permet aujourd'hui de résumer des ouvrages entiers, de traduire instantanément entre des dizaines de langues ou d'assister les développeurs dans l'écriture de code.\n\n" +
            "Chapitre IV — Limites, biais et responsabilité humaine\n" +
            "Aussi performants soient-ils, les modèles d'intelligence artificielle ne possèdent ni conscience ni compréhension réelle du monde : ils calculent des probabilités. Ils peuvent ainsi inventer des faits avec assurance (phénomène d'hallucination) ou reproduire les biais présents dans leurs corpus d'apprentissage. L'esprit critique et la supervision humaine demeurent donc indispensables.",
        "Les Heures Bleues" =>
            "Chapitre I — Le ferry de Tanger\n" +
            "Le détroit avait cette couleur d'ardoise mouillée qu'il ne prend qu'à la fin du mois d'août, juste avant que le vent d'est ne balaie les dernières brumes de l'été. Accoudée au bastingage du pont supérieur, Nadia observait la ligne blanche des remparts de Tanger qui grandissait lentement à l'horizon. Cela faisait neuf ans qu'elle n'avait pas posé le pied sur le port de sa ville natale. Dans son sac de toile, entre deux carnets de traduction annotés au crayon, reposait la clé en laiton de la maison de la rue des Siaghines.\n\n" +
            "Son oncle Driss lui avait laissé l'enveloppe chez le notaire sans un mot d'explication, sinon une phrase écrite de sa main tremblante : « Ouvre les volets avant que l'humidité n'emporte les livres. » Driss avait passé quarante ans à collectionner des éditions bilingues, des cartes marines et des correspondances oubliées.\n\n" +
            "Chapitre II — La maison aux volets bleus\n" +
            "La serrure résista d'abord, grippée par le sel marin, puis céda dans un claquement sec. À l'intérieur, l'air sentait le cèdre ancien, le papier jauni et le jasmin sec tombé dans le patio. La lumière de cinq heures du soir traversait les moucharabiehs et dessinait sur les zelliges des motifs géométriques mouvants.\n\n" +
            "Sur le bureau d'acajou du premier étage, un manuscrit relié de cuir vert était resté ouvert à la page quarante-deux. En marge d'un poème traduit de l'espagnol, Nadia reconnut l'écriture fine de sa mère, disparue lorsqu'elle avait douze ans : c'était une correspondance secrète échangée entre Tanger et Cadix dans les années soixante-dix.\n\n" +
            "Chapitre III — Les heures bleues\n" +
            "Chaque soir, entre le coucher du soleil et la première étoile, Tanger basculait dans cette lumière suspendue que les peintres appellent l'heure bleue. Nadia prit l'habitude de s'installer sur la terrasse avec une théière de menthe fraîche et les cahiers de sa mère. À la fin de l'été, le manuscrit était entièrement transcrit : elle savait désormais que certains retours sont le seul moyen d'avancer avec toute son histoire.",

               "Le Jardin de Fès" =>
            "Chapitre I — L'ombre du figuier\n" +
            "Au fond de l'impasse El-Attarine, derrière une porte de cèdre cloutée que rien ne distinguait des murs ocre de la vieille médina de Fès, s'ouvrait un jardin secret que trois générations d'artisans avaient façonné avec patience. Au centre de la cour trônait un figuier centenaire dont les branches noueuses couvraient presque entièrement la fontaine octogonale en zellige vert et blanc. Tout autour, des parterres de menthe poivrée, de verveine, de bigaradiers et de rosiers anciens embaumaient l'air dès les premières heures du jour.\n\n" +
            "Hadj Omar, maître-artisan dinandier reconnu dans tout le quartier des Seffarine, y prenait son café chaque matin à l'aube, écoutant le ruissellement régulier de l'eau qui alimentait le bassin depuis les canaux souterrains de l'oued Fès. Pour lui, ce jardin n'était pas un simple décor : c'était la mémoire vivante de sa famille, un refuge silencieux où les bruits et les querelles du souk s'apaisaient dès le seuil franchi.\n\n" +
            "Chapitre II — Les trois héritiers\n" +
            "Lorsque Hadj Omar décida de transmettre la demeure familiale, trois visions très différentes s'affrontèrent autour de la table basse en cuivre ciselé. Karim, l'aîné, architecte formé à Rabat, souhaitait restaurer le riad pour en faire une maison d'hôtes contemporaine ouverte aux voyageurs du monde entier. Sa sœur Meryem, chercheuse en botanique, refusait catégoriquement qu'on déplace la moindre pierre ou qu'on touche aux racines du figuier et aux plantes médicinales cultivées par leur grand-mère.\n\n" +
            "Quant au benjamin, Sami, il n'avait pas encore donné son avis. Intrigué par une trappe dissimulée sous l'établi de l'atelier du sous-sol, il venait de mettre au jour un coffre en bois de thuya contenant les plans originaux de la demeure datés de 1894, accompagnés d'un carnet calligraphié par leur arrière-grand-père.\n\n" +
            "Chapitre III — Le secret des canaux\n" +
            "En dépliant le parchemin jaunissant sous la lumière de la lampe d'atelier, les trois frères et sœur comprirent que la maison et le jardin ne faisaient qu'un. Les plans révélaient un ingénieux système hydraulique et bioclimatique : l'eau circulant sous les parterres de fleurs et le feuillage dense du figuier abaissaient naturellement la température des salons de près de huit degrés durant les étés torrides de Fès.\n\n" +
            "Modifier l'agencement des bassins ou abattre les arbres pour agrandir les chambres aurait brisé cet équilibre centenaire et condamné la fraîcheur naturelle de toute la maison.\n\n" +
            "Chapitre IV — L'accord retrouvé\n" +
            "Bouleversés par cette découverte, Karim, Meryem et Sami renoncèrent à leurs querelles et choisirent d'unir leurs savoir-faire. Karim dessina une restauration fidèle aux techniques traditionnelles de plâtre ciselé et de bois de cèdre ; Meryem créa un conservatoire botanique répertoriant les quarante espèces végétales du patio ; et Sami rouvrit l'atelier de dinanderie pour former de jeunes apprentis de la médina.\n\n" +
            "Le jour de la réouverture, lorsque l'eau claire recommença à chanter dans la vasque centrale sous les feuilles du figuier, tout le voisinage sut que l'âme du jardin de Fès venait de renaître.",


        "Nuit sur l'Atlas" =>
            "Chapitre I — Le col du Tizi n'Tichka\n" +
            "À seize heures, les premiers flocons commencèrent à tourbillonner autour des lacets de la route nationale. Le minibus reliant Ouarzazate à Marrakech grimpait péniblement vers le col à plus de deux mille deux cents mètres d'altitude. À bord, six passagers que rien ne destinait à voyager ensemble : un médecin de campagne, une professeure de musique, deux étudiants en géologie, un marchand d'épices et un vieux berger silencieux.\n\n" +
            "Chapitre II — Le refuge de pierre\n" +
            "Guidés par le berger qui connaissait chaque repli de la montagne, les voyageurs rejoignirent à pied une ancienne maison cantonnière en pierre sèche. Avec quelques branches de genévrier sec, ils allumèrent un feu dans l'âtre noirci. Dehors, le blizzard hurlait ; dedans, autour de la flamme, chacun raconta son histoire tandis que la musicienne jouait une mélodie douce au oud.\n\n" +
            "Chapitre III — L'aube blanche\n" +
            "Au lever du jour, la tempête s'était tue. Un silence cristallin enveloppait les sommets immaculés baignés d'une lumière rose. Avant de redescendre vers la plaine, les six voyageurs échangèrent leurs adresses, liés par une fraternité durable.",

        "Routes de la soie" =>
            "Chapitre I — Bien plus que de la soie\n" +
            "Les Routes de la soie désignent un vaste réseau d'itinéraires terrestres et maritimes reliant l'Asie orientale à la Méditerranée dès le IIe siècle avant notre ère. Les marchands y transportaient la soie chinoise, mais aussi les épices d'Inde, le verre, la porcelaine, l'encens et le papier.\n\n" +
            "Chapitre II — Les villes-oasis et les passeurs de savoirs\n" +
            "De Xi'an à Samarcande, de Bagdad à Fès et Venise, les caravansérails étaient de véritables carrefours intellectuels où se croisaient savants, traducteurs et astronomes. C'est ainsi que la fabrication du papier gagna Samarcande au VIIIe siècle puis tout le bassin méditerranéen.\n\n" +
            "Chapitre III — L'héritage d'un monde connecté\n" +
            "Les Routes de la soie rappellent que la circulation pacifique des idées, des sciences et des arts a toujours été le moteur le plus fécond du progrès humain.",

        "Al-Andalus, mémoire vive" =>
            "Chapitre I — Cordoue, capitale des bibliothèques\n" +
            "Au Xe siècle, Cordoue abritait plus de soixante-dix bibliothèques publiques et privées. La plus célèbre, rassemblée par le calife Al-Hakam II, réunissait près de quatre cent mille volumes catalogués avec soin par des copistes, des relieurs et des enlumineurs.\n\n" +
            "Chapitre II — Le pont entre deux rives\n" +
            "Les textes grecs d'Aristote, d'Euclide et de Galien, traduits et commentés en arabe par des penseurs comme Ibn Rochd (Averroès), furent ensuite traduits en latin à Tolède et diffusés dans toute l'Europe, forgeant un patrimoine scientifique, architectural et musical partagé entre Cordoue, Séville, Grenade et Fès.\n\n" +
            "Chapitre III — Une mémoire toujours vivante\n" +
            "L'histoire d'Al-Andalus montre comment le dialogue des cultures et le respect du savoir peuvent engendrer des siècles de créativité.",
        "Cosmos pour tous" =>
            "Chapitre I — Lever les yeux vers la nuit\n" +
            "Loin des lumières des grandes villes, par une nuit sans lune au cœur du désert, le ciel révèle près de trois mille étoiles visibles à l'œil nu. Notre galaxie, la Voie lactée, n'est qu'une île parmi des centaines de milliards d'autres galaxies peuplant l'Univers observable. Regarder le ciel, c'est toujours regarder dans le passé : la lumière du Soleil met huit minutes à nous parvenir, et celle de Proxima du Centaure plus de quatre ans.\n\n" +
            "Chapitre II — La vie et la mort des étoiles\n" +
            "Les étoiles naissent au sein d'immenses nuages de gaz appelés nébuleuses. Sous l'effet de la gravité, la matière se contracte jusqu'à atteindre dix millions de degrés au cœur : la fusion nucléaire s'allume. En fin de vie, les étoiles les plus massives explosent en supernovas, dispersant dans l'espace le carbone, l'oxygène et le fer qui composent nos planètes et notre propre corps.\n\n" +
            "Chapitre III — Les trous noirs et les frontières du savoir\n" +
            "Lorsqu'une étoile géante s'effondre sur elle-même, la gravité devient si intense que même la lumière ne peut s'en échapper. Aujourd'hui détectés grâce aux ondes gravitationnelles, les trous noirs confirment les prédictions les plus audacieuses de la relativité générale.",

        "Penser le temps" =>
            "Chapitre I — Le paradoxe d'Augustin\n" +
            "« Qu'est-ce donc que le temps ? Si personne ne me le demande, je le sais ; mais si on me le demande et que je veuille l'expliquer, je ne le sais plus. » Nous mesurons chaque seconde avec nos montres, mais la nature intime du temps échappe à nos sens.\n\n" +
            "Chapitre II — Temps objectif et temps vécu\n" +
            "La physique mesure un temps uniforme et mathématique, mais la conscience humaine vit une durée élastique : une heure d'attente paraît interminable, tandis qu'une heure de lecture passionnante s'envole en un instant.\n\n" +
            "Chapitre III — Habiter le présent\n" +
            "Apprendre à penser le temps, c'est retrouver la maîtrise de son attention et redonner à chaque instant sa juste profondeur.",

               "Petit traité du doute" =>
            "Chapitre I — L'éloge de la question\n" +
            "Dans le langage courant, on confond souvent le doute avec l'hésitation paralysante, le manque de confiance en soi ou le scepticisme stérile qui consiste à tout rejeter en bloc. Pourtant, dans l'histoire des sciences comme en philosophie, le doute méthodique est exactement l'inverse : c'est le premier pas vers la lucidité et la liberté de pensée. Douter, ce n'est pas renoncer à la vérité ; c'est refuser de se contenter d'idées reçues sans les avoir soumises à l'examen de la raison.\n\n" +
            "Au XVIIe siècle, René Descartes fit du doute un véritable outil de tri intellectuel : mettre provisoirement entre parenthèses tout ce qui paraît incertain ou hérité par habitude, afin de découvrir ce qui résiste solidement à l'analyse critique.\n\n" +
            "Chapitre II — Les pièges de la certitude immédiate\n" +
            "L'esprit humain aime le confort des réponses rapides et définitives. Face à une situation complexe, notre cerveau cherche spontanément un raccourci pour réduire l'incertitude. Les psychologues appellent « biais de confirmation » cette tendance naturelle à ne retenir que les faits qui confortent nos opinions déjà formées, tout en ignorant ou en minimisant les preuves qui les contredisent.\n\n" +
            "À l'ère des réseaux sociaux et de l'information instantanée, où les rumeurs émotionnelles circulent bien plus vite que les vérifications rigoureuses, apprendre à suspendre son jugement pendant quelques minutes devient une véritable discipline citoyenne.\n\n" +
            "Chapitre III — L'art de vérifier et de nuancer\n" +
            "Pratiquer un doute sain repose sur trois réflexes simples. Le premier consiste à remonter à la source d'une affirmation : qui parle, sur quelles preuves s'appuie-t-il, et ses données sont-elles vérifiables ? Le deuxième consiste à chercher activement l'argument opposé le plus solide, afin de tester la robustesse de sa propre opinion.\n\n" +
            "Enfin, le troisième réflexe consiste à accepter les nuances : entre le « tout blanc » et le « tout noir », la réalité se situe presque toujours dans une zone intermédiaire qui demande de la mesure et de la précision.\n\n" +
            "Chapitre IV — Décider avec lucidité\n" +
            "Contrairement à une crainte répandue, le doute constructif ne mène pas à l'inaction : il éclaire et sécurise la décision. Une fois les hypothèses confrontées, les risques évalués et les angles morts identifiés, l'action devient plus juste, plus responsable et capable de s'ajuster en cours de route si de nouveaux faits apparaissent.\n\n" +
            "Savoir dire « je ne sais pas encore, vérifions » n'est jamais un aveu de faiblesse : c'est la marque d'un esprit rigoureux.",


        "Concevoir une API durable" =>
            "Chapitre I — Une API est un contrat\n" +
            "Exposer une API REST sur le web revient à signer un contrat clair avec les applications clientes. Des routes prévisibles, le respect des verbes HTTP (GET, POST, PUT, DELETE) et des codes de statut explicites facilitent l'intégration et la maintenance.\n\n" +
            "Chapitre II — Sécurité et validation par défaut\n" +
            "Une architecture durable valide chaque entrée via des DTOs, prévient les injections SQL grâce aux requêtes paramétrées d'Entity Framework Core, hache les mots de passe avec BCrypt et protège chaque endpoint sensible par des rôles JWT précis.\n\n" +
            "Chapitre III — Évoluer sans casser l'existant\n" +
            "La pagination des listes, la journalisation structurée et la clarté des modèles garantissent la pérennité d'un backend en production.",

        "Réseaux sans mystère" =>
            "Chapitre I — Ce qui se passe quand vous tapez une URL\n" +
            "Lorsqu'on saisit une adresse web, le protocole DNS traduit le nom de domaine en adresse IP, TCP établit une connexion fiable et TLS chiffre les échanges en HTTPS en quelques millisecondes.\n\n" +
            "Chapitre II — Le modèle en couches\n" +
            "Le découpage en couches (physique, réseau IP, transport TCP, application HTTP) permet à une application web de fonctionner indifféremment en Wi-Fi, en 5G ou sur fibre optique.\n\n" +
            "Chapitre III — Diagnostic et bonnes pratiques\n" +
            "Maîtriser les codes HTTP (200, 401, 403, 404, 500) et les règles CORS permet de diagnostiquer rapidement toute communication entre React et ASP.NET Core.",

        "Sable et lumière" =>
            "Chapitre I — L'alphabet des dunes\n" +
            "Le vent du sud écrit chaque matin\nUne page neuve sur l'or du désert,\nNul pas ne dure plus d'une heure\nSous le grand ciel où le silence respire.\n\n" +
            "Là où la ville s'arrête et doute,\nLa dune enseigne la patience :\nElle épouse l'air sans jamais rompre,\nEt garde la chaleur du jour au creux de l'ombre.\n\n" +
            "Chapitre II — La ville blanche et l'océan\n" +
            "Sur les remparts battus par l'écume,\nLe sel dessine des dentelles blanches.\nLes mouettes tracent des arcs d'argent\nEntre les barques bleues et l'horizon calme.\n\n" +
            "Chapitre III — Ce qui demeure\n" +
            "Les saisons passent comme des voyageurs,\nLes mots s'assemblent puis s'effacent,\nMais il suffit d'une page ouverte le soir\nPour rallumer le soleil au milieu de l'hiver.",

        _ =>
            $"Prologue — {title}\n" +
            $"{(string.IsNullOrWhiteSpace(desc) ? "Un ouvrage de référence dans notre bibliothèque numérique." : desc)}\n\n" +
            $"Chapitre I — Découverte\n" +
            $"Dans ce premier chapitre, {author} pose les bases de sa démarche et invite le lecteur à explorer chaque thème en profondeur."

            
    };
}
