import { Node } from '@tiptap/core'

export default class Iframe extends Node {
    get name() {
        return 'iframe'
    }

    get schema() {
        return {
            attrs: {
                src: {
                    default: null
                }
            },
            group: 'block',
            selectable: false,
            parseDOM: [
                {
                    tag: 'iframe',
                    getAttrs: dom => ({
                        src: dom.getAttribute('src')
                    })
                }
            ],
            toDOM: node => [
                'iframe',
                {
                    src: node.attrs.src,
                    frameborder: 0,
                    allowfullscreen: 'true'
                }
            ]
        }
    }

    get view() {
        return {
            props: ['node', 'updateAttrs', 'editable'],
            computed: {
                src: {
                    get() {
                        return this.node.attrs.src // eslint-disable-line
                    },
                    set(src) {
                        this.updateAttrs({
                            src
                        })
                    }
                }
            },
            template: `
        <div class='iframe q-video q-video--responsive' style='padding-bottom: 56.25%'>
          <iframe class='iframe__embed' :src='src'></iframe>
          <input class='iframe__input' @paste.stop type='text' v-model='src' v-if='editable' />
        </div>
      `
        }
    }
}
