/**
 * TipTap extension to add support for rendering external video.
 * See: https://github.com/joevallender/tiptap2-image-example/blob/main/src/extensions/external-video.js
 */

import { Node, mergeAttributes } from '@tiptap/core'

export interface VideoOptions {
    inline: boolean,
    HTMLAttributes: Record<string, any>,
}

declare module '@tiptap/core' {
    interface Commands<ReturnType> {
        video: {
            /**
             * Add an image
             */
            setVideo: (options: {
                src: string,
                title?: string,
                frameborder?: number,
                allow?: string,
                allowfullscreen?: string
            }) => ReturnType,
        }
    }
}

export const Video = Node.create<VideoOptions>({
    name: 'video',

    inline() {
        return this.options.inline
    },

    group() {
        return this.options.inline ? 'inline' : 'block'
    },

    draggable: true,

    addAttributes() {
        return {
            src: {
                default: null,
            },
            title: {
                default: null,
            },
            frameborder: {
                default: '0',
            },
            allow: {
                default: 'accelerometer; autoplay; clipboard-write; encrypted-media; gyroscope; picture-in-picture'
            },
            allowfullscreen: {
                default: 'allowfullscreen'
            },
            class: {
                default: 'iframe__embed'
            }
        }
    },

    parseHTML() {
        return [
            {
                tag: 'iframe[src]',
            },
        ]
    },

    renderHTML({ HTMLAttributes }) {
        /*
        <div class='iframe q-video q-video--responsive' style='padding-bottom: 56.25%'>
          <iframe class='iframe__embed' :src='src'></iframe>
          <input class='iframe__input' @paste.stop type='text' v-model='src' v-if='editable' />
        </div>
        */

        return [
            'div',
                { class: 'iframe q-video q-video--responsive', style: 'padding-bottom: 56.25%' },
                [
                    'iframe',
                    mergeAttributes(this.options.HTMLAttributes, HTMLAttributes)
                ]
            ]
    },

    addCommands() {
        return {
            setVideo: options => ({ commands }) => {
                return commands.insertContent({
                    type: this.name,
                    attrs: options,
                })
            },
        }
    },
})