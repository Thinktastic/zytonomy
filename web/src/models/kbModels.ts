import { marked } from 'marked';

// https://marked.js.org/#demo
marked.setOptions({
    breaks: false,
    gfm: true,
    headerIds: true,
    headerPrefix: '',
    langPrefix: 'language-',
    mangle: true,
    pedantic: false,
    sanitize: false,
    silent: false,
    smartLists: false,
    smartypants: false,
    xhtml: false
})

/**
 * The metadata entry assocated to a KB answer
 */
 export type KbMetadata = {
    /**
     * The name of the metadata field
     */
    name: string

    /**
     * The value associated with the metadata field
     */
    value: string
}

/**
 * Followup prompt which can be associated with a KB response context.
 */
export type KbFollowupPrompt = {
    /**
     * The disdplay order for the prompt.
     */
    displayOrder: number

    /**
     * The direct ID of the Q&A entry
     */
    qnaId: string

    /**
     * The display text for the prompt
     */
    displayText: string
}

/**
 * The answer context which can contain followup prompts.
 */
export type KbContext = {
    /**
     * The prompts which contain the followup questions if present.
     */
    prompts: Array<KbFollowupPrompt>
}

/**
 * Corresponds to a knowledgebase answer from an Azure Q&A Maker response
 * https://docs.microsoft.com/en-us/rest/api/cognitiveservices/qnamaker4.0/runtime/generateanswer#qnasearchresultlist
 */
export type KbAnswer = {
    /**
     * The ID corresponding to the direct entry in the Q&A Maker repository.
     * Used for updating and training the repository.
     */
    id: number

    /**
     * The string which contains the answer in Markdown format.
     */
    answer: string

    /**
     * The decimal score of the response.
     */
    score: number

    /**
     * The name of the source document that this response came from
     */
    source: string

    /**
     * The metadata array associated with this entry.
     */
    metadata: Array<KbMetadata>

    /**
     * The context which can include the additional prompts for the response.
     */
    context: KbContext

    /**
     * The questions that are mapped to thie answer entry.
     */
    questions: string[]
}

/**
 * Root level object for the KB response which contains the answers.
 */
export type KbResponse = {
    /**
     * The answers matching the response.
     */
    answers: Array<KbAnswer>
}

/**
 * View model representation of a KB entry for the front-end binding.
 */
export type KbEntry = {
    /**
     * The primary question associated with this entry.
     */
    title: string

    /**
     * The followup prompts for this entry.
     */
    prompts: Array<KbFollowupPrompt>

    /**
     * The KB ID assocaited with this entry.
     */
    kbId: number

    /**
     * The document name source of the result
     */
    source: string

    /**
     * The relevance of the response when it is a KB response.
     */
    relevance: number

    /**
     * The HTML body of this response.
     */
    bodyHtml: string
}