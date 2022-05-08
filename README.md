# Thinktastic Zytonomy

- **[Zy](https://en.wiktionary.org/wiki/zy)**: Interrogative; "which"
- **[-nomy](https://en.wiktionary.org/wiki/-nomy#English)**: a system of rules, laws, or knowledge about a body of a particular field

Zytonomy is an open source, team-oriented, collaborative knowledge management tool built on top of [Azure Cognitive Question Answering](https://azure.microsoft.com/en-us/services/cognitive-services/question-answering/).  In particular, it provides a team-based context and front-end web application to work with a knowledge base built on top of Azure Cognitive Question Answering.

Many teams have an existing corpus of knowledge sitting in **documents**.  This is particularly true for information like:

- Standard operating procedures
- Laws, policies, and regulations
- Onboarding documentation such as processes
- Protocol documentation supporting regulated processes

Zytonomy's unique super power is its ability to surface this information in a team-based context using *existing* sources of content.

Azure Cognitive Question Answering is a service which provides the ability to:

- Ingest unstructured document sources
- Provide NLP-based search across the content of the documents
- Surface answers from deep within the documents
- Learn through feedback which answers match which questions

Zytonomy provides a front-end and service to work with these powerful features for your team!

## Project

The following sections describe the architecture and structure of the project.

For more detailed documentation, see the `/docs` directory.

```
cd docs
yarn
yarn start
```

The documentation will be available at `http://localhost:3001`.

### Architecture

Zytonomy is built on top of Azure using the following technologies:

- **Azure Functions**: This is the serverless backend of the application written in C# (.NET 6).
- **Azure CosmosDB**: This is the document-oriented datastore backing the application.
- **Vue 3**: This is the frontend of the application using Vue.js with [the Quasar framework](https://quasar.dev/).  The code is written in TypeScript.

### Structure

- `/api`: The Azure Functions server app.
- `/docs`: The Docusaurus documentation on how to deploy and use the application.
- `/web`: The Vue 3 Quasar app which is deployed to static hosting.

## License

See the LICENSE file for license information.

## FAQ

**Is it free for commercial use?**
Yes!

**Is there a SaaS version?**
No.  Right now, there is only a demo version.  The intent is to eventually have a hosted version that any team can sign up for and start using.  Please reach out to me if you're interested!