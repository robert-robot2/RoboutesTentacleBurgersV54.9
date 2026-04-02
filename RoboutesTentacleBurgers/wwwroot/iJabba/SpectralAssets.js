








window.SpectralLandscape = {
    _saveTimer: null,

    save: function (json) {
        // Debounce — clear any pending save first
        if (this._saveTimer) clearTimeout(this._saveTimer);
        this._saveTimer = setTimeout(() => {
            try {
                localStorage.setItem('spectralx_landscape', json);
                console.log('[Landscape] Saved —', json.length, 'bytes');
            } catch (e) {
                console.warn('[Landscape] Save failed:', e);
            }
            this._saveTimer = null;
        }, 500);
    },

    load: function () {
        try {
            const data = localStorage.getItem('spectralx_landscape');
            if (!data) {
                console.log('[Landscape] No saved data found — using default');
                return null;
            }
            console.log('[Landscape] Loaded —', data.length, 'bytes');
            return data;
        } catch (e) {
            console.warn('[Landscape] Load failed:', e);
            return null;
        }
    },

    clear: function () {
        localStorage.removeItem('spectralx_landscape');
      //  console.log('[Landscape] Save cleared');
    }
};

window.SpectralTextSystem = (function () {
    const _atlases = {};
    const _atlasTextures = {};
    const _atlasImages = {};  // ADD — cache image for re-upload on context switch

 

    function reset() {
      //  console.log("[TextSystem] Resetting textures after context change");

        for (const key in _atlasImages) {
            _atlasTextures[key] = _uploadAtlasTexture(_atlasImages[key]);
        }
    }


    async function loadAtlas(fontKey, jsonUrl, texUrl) {


        if (_atlases[fontKey] && _atlasImages[fontKey]) {
            // JSON already loaded but texture might be stale — re-upload
            _atlasTextures[fontKey] = _uploadAtlasTexture(_atlasImages[fontKey]);
            return;
        }



      //  console.log('[TextSystem] Loading atlas:', fontKey);

        const [jsonRes, img] = await Promise.all([
            fetch(jsonUrl).then(r => r.json()),
            new Promise((res, rej) => {
                const i = new Image();
                i.onload = () => res(i);
                i.onerror = rej;
                i.src = texUrl;
            })
        ]);


        const tex = _uploadAtlasTexture(img);


        _atlases[fontKey] = jsonRes;
        _atlasTextures[fontKey] = tex;
        _atlasImages[fontKey] = img;  
      //  console.log('[TextSystem] Atlas ready:', fontKey,
        //    'glyphs:', Object.keys(jsonRes.glyphs || {}).length);
    }










    function _uploadAtlasTexture(img) {
        const canvas = document.getElementById('SpectralX-Viewport');
        if (!canvas) return null;
        const gl = canvas.getContext('webgl2') || canvas.getContext('webgl');
        if (!gl) return null;
        const tex = gl.createTexture();
        gl.bindTexture(gl.TEXTURE_2D, tex);
        gl.pixelStorei(gl.UNPACK_PREMULTIPLY_ALPHA_WEBGL, false);
        gl.texImage2D(gl.TEXTURE_2D, 0, gl.RGBA, gl.RGBA, gl.UNSIGNED_BYTE, img);
        gl.texParameteri(gl.TEXTURE_2D, gl.TEXTURE_MIN_FILTER, gl.LINEAR);
        gl.texParameteri(gl.TEXTURE_2D, gl.TEXTURE_MAG_FILTER, gl.LINEAR);
        gl.texParameteri(gl.TEXTURE_2D, gl.TEXTURE_WRAP_S, gl.CLAMP_TO_EDGE);
        gl.texParameteri(gl.TEXTURE_2D, gl.TEXTURE_WRAP_T, gl.CLAMP_TO_EDGE);
        return tex;
    }

    function buildTextGeometry(meshId, text, fontKey, fontSize, letterSpacing, align) {
        const atlas = _atlases[fontKey];
        if (!atlas) {
          //  console.warn('[TextSystem] Atlas not loaded:', fontKey);
            return false;
        }

        const glyphMap = {};
        for (const g of atlas.glyphs) {
            glyphMap[g.unicode] = g;
        }

        const metrics = atlas.metrics;
        const atlaW = atlas.atlas.width;
        const atlaH = atlas.atlas.height;
        const scale = fontSize / metrics.emSize;

        const verts = [];
        const uvs = [];
        const norms = [];

        // First pass — measure total width for alignment
        let totalWidth = 0;
        for (const ch of text) {
            const g = glyphMap[ch.charCodeAt(0)];
            if (!g) continue;
            totalWidth += (g.advance + letterSpacing) * scale;
        }

        let offsetX = align === 1 ? -totalWidth / 2 :  // Center
            align === 2 ? -totalWidth : 0;    // Right

        // Second pass — build quads
        for (const ch of text) {
            const g = glyphMap[ch.charCodeAt(0)];
            if (!g || !g.planeBounds) {
                if (g) offsetX += g.advance * scale;
                else offsetX += fontSize * 0.3;
                continue;
            }

            if (g.planeBounds && g.atlasBounds) {
                const pb = g.planeBounds;
                const ab = g.atlasBounds;

                const x0 = offsetX + pb.left * scale;
                const x1 = offsetX + pb.right * scale;
                const y0 = pb.bottom * scale;
                const y1 = pb.top * scale;

                const u0 = ab.left / atlaW;
                const u1 = ab.right / atlaW;
                const v0 = 1 - ab.top / atlaH;
                const v1 = 1 - ab.bottom / atlaH;

                // Triangle 1
                verts.push(x0, y0, 0, x1, y0, 0, x1, y1, 0);
                uvs.push(u0, v0, u1, v0, u1, v1);
                // Triangle 2
                verts.push(x0, y0, 0, x1, y1, 0, x0, y1, 0);
                uvs.push(u0, v0, u1, v1, u0, v1);

                for (let i = 0; i < 6; i++)
                    norms.push(0, 0, 1);
            }

            offsetX += (g.advance + letterSpacing) * scale;
        }

        if (verts.length === 0) {
         //   console.warn('[TextSystem] No geometry built for:', text);
            return false;
        }

        window.SpectralGLInterop.uploadParsedMesh(meshId, {
            vertices: new Float32Array(verts),
            normals: new Float32Array(norms),
            uvs: new Float32Array(uvs),
            matBreaks: [verts.length / 3],
            matIndices: [0],
        }, [], []);

     //   console.log('[TextSystem] Built:', meshId,
          //  'chars:', text.length, 'verts:', verts.length / 3);
        return true;
    }

    function getAtlasTexture(fontKey) {
        return _atlasTextures[fontKey] || null;
    }

    function isAtlasLoaded(fontKey) {
        return !!_atlases[fontKey];
    }

    return { loadAtlas, buildTextGeometry, getAtlasTexture, isAtlasLoaded,reset };
})();









// FBX Parser JS Helper


window.SpectralFBXHelper = {

    loadMesh: async function (url) {
        try {
            // Native fetch — bypasses WASM HTTP entirely
            const response = await fetch(url);
            if (!response.ok) return null;

            const buffer = await response.arrayBuffer();
            const view = new DataView(buffer);

            // Validate FBX binary magic header
            const magic = new Uint8Array(buffer, 0, 18);
            const header = String.fromCharCode(...magic);
            if (!header.startsWith('Kaydara FBX Binary')) {
                console.warn('[FBXHelper] Not binary FBX:', url);
                return null;
            }

            // FBX version at byte 23
            const version = view.getUint32(23, true);
            console.log('[FBXHelper] Parsing:', url, 'version:', version);

            const result = await this.parseFBX(buffer, view, version);

            // Null check — handles FBX files with no geometry or empty vertex arrays
            if (!result || !result.vertices || result.vertices.length === 0) {
                console.warn('[FBXHelper] Empty result for:', url);
                return null;
            }

            console.log('[FBXHelper] Done:', url,
                'verts:', result.vertices.length / 3,
                'faces:', result.rawIndices.length);

            return result;

        } catch (ex) {
            console.error('[FBXHelper] Failed:', url, ex);
            return null;
        }
    },

    parseFBX: async function (buffer, view, version) {
        try {
            const nodes = [];
            let offset = 27;
            const byteLength = buffer.byteLength;

            while (offset < byteLength - 160) {
                const node = this.readNode(buffer, view, offset, version);
                if (!node || node.endOffset === 0) break;
                nodes.push(node);
                offset = node.endOffset;
            }

            const objectsNode = nodes.find(n => n.name === 'Objects');
            if (!objectsNode) return null;

            const geomNode = objectsNode.children.find(n => n.name === 'Geometry');
            if (!geomNode) return null;

            const vertNode = geomNode.children.find(n => n.name === 'Vertices');
            const vertices = vertNode ? await this.readDoubleArray(vertNode) : [];

            const idxNode = geomNode.children.find(n => n.name === 'PolygonVertexIndex');
            const rawIndices = idxNode ? await this.readIntArray(idxNode) : [];

            const uvLayerNode = geomNode.children.find(n => n.name === 'LayerElementUV');
            let uvs = [], uvIndices = [];
            if (uvLayerNode) {
                const uvNode = uvLayerNode.children.find(n => n.name === 'UV');
                const uvIdxNode = uvLayerNode.children.find(n => n.name === 'UVIndex');
                uvs = uvNode ? await this.readDoubleArray(uvNode) : [];
                uvIndices = uvIdxNode ? await this.readIntArray(uvIdxNode) : [];
            }

            const normLayerNode = geomNode.children.find(n => n.name === 'LayerElementNormal');
            let normals = [], normalIndices = [];
            if (normLayerNode) {
                const normNode = normLayerNode.children.find(n => n.name === 'Normals');
                const normIdxNode = normLayerNode.children.find(n => n.name === 'NormalsIndex');
                normals = normNode ? await this.readDoubleArray(normNode) : [];
                normalIndices = normIdxNode ? await this.readIntArray(normIdxNode) : [];
            }

            const matLayerNode = geomNode.children.find(n => n.name === 'LayerElementMaterial');
            let materialIndices = [];
            if (matLayerNode) {
                const matIdxNode = matLayerNode.children.find(n => n.name === 'Materials');
                materialIndices = matIdxNode ? await this.readIntArray(matIdxNode) : [];
            }

            // NEW — extract embedded textures
            const textures = this.extractTextures(nodes, objectsNode);
            const materialColors = this.extractMaterialColors(nodes, objectsNode);

            return {
                vertices, rawIndices, uvs, uvIndices,
                normals, normalIndices, materialIndices,
                textures,
                materialColors  // NEW
            };

        } catch (ex) {
            console.error('[FBXHelper] parseFBX error:', ex);
            return null;
        }
    },

    loadAndUpload: async function (url, meshName) {
        try {
            // Wait for GL to be ready — uploadParsedMesh needs _gl initialized
            if (!window.SpectralGLInterop) {
                console.warn('[FBXHelper] SpectralGLInterop not ready for:', meshName);
                return false;
            }

            const raw = await this.loadMesh(url);
            if (!raw) {
                console.warn('[FBXHelper] loadMesh returned null for:', meshName);
                return false;
            }

            const processed = this.processMesh(raw);
            if (!processed) {
                console.warn('[FBXHelper] processMesh returned null for:', meshName);
                return false;
            }
            if (meshName === 'Bush001') {
                console.log('[Bush Full Debug]',
                    'raw verts:', raw.vertices.length,
                    'raw indices:', raw.rawIndices.length,
                    'processed verts:', processed.vertices.length,
                    'matBreaks:', processed.matBreaks,
                    'matIndices:', processed.matIndices,
                    'textures count:', raw.textures?.length,
                    'materialColors:', raw.materialColors,
                    'textures[0] exists:', !!raw.textures?.[0],
                    'textures[0] length:', raw.textures?.[0]?.length
                );
            }
            // Upload directly to WebGL — no C# round-trip for geometry
            window.SpectralGLInterop.uploadParsedMesh(meshName, processed, raw.textures || [], raw.materialColors || []);

            console.log('[FBXHelper] loadAndUpload success:', meshName,
                'verts:', processed.vertices.length / 3);
            return true;

        } catch (ex) {
            console.error('[FBXHelper] loadAndUpload failed:', meshName, ex);
            return false;
        }
    },

    loadAllAndUploadJson: async function (meshListJson) {
        // Parse the JSON string — guarantees a real JS array regardless of
        // how Blazor serialized the C# anonymous type array
        const meshList = JSON.parse(meshListJson);
        return await this.loadAllAndUpload(meshList);
    },

    loadAllAndUpload: async function (meshList) {
        console.log('[FBXHelper] loadAllAndUpload starting —', meshList.length, 'meshes');

        // Wait for GL to be ready — poll until _gl is initialized
        // REPLACE the existing await new Promise block with:
        await new Promise(resolve => {
            const check = () => {
                if (window.SpectralGLInterop &&
                    document.getElementById('SpectralX-Viewport')) {
                    resolve();
                } else {
                    setTimeout(check, 50);
                }
            };
            check();
        });
        // All fetches run in parallel — browser handles concurrent requests
        const results = await Promise.all(
            meshList.map(m => this.loadAndUpload(m.url, m.name))
        );

        const succeeded = results.filter(r => r === true).length;
        const failed = results.filter(r => r === false).length;

        console.log('[FBXHelper] loadAllAndUpload complete —',
            succeeded, 'succeeded,', failed, 'failed');

        return results;
    },




    extractTextures: function (rootNodes, objectsNode) {
        // Step 1 — find embedded video/image content
        const videoDataUrls = {};
        const videoNodes = objectsNode.children.filter(n => n.name === 'Video');

        for (const videoNode of videoNodes) {
            const contentNode = videoNode.children.find(n => n.name === 'Content');
            if (!contentNode) continue;

            const offset = contentNode.propsStart;
            const view = contentNode.view;
            const typeCode = String.fromCharCode(view.getUint8(offset));
            if (typeCode !== 'R') continue;

            const byteLen = view.getUint32(offset + 1, true);
            if (byteLen === 0) continue;

            const bytes = new Uint8Array(contentNode.buffer, offset + 5, byteLen);
            let mimeType = 'image/png';
            if (bytes[0] === 0xFF && bytes[1] === 0xD8) mimeType = 'image/jpeg';

            // Convert to base64
            let binary = '';
            const chunkSize = 8192;
            for (let i = 0; i < bytes.length; i += chunkSize) {
                binary += String.fromCharCode(...bytes.subarray(i, i + chunkSize));
            }
            const dataUrl = `data:${mimeType};base64,${btoa(binary)}`;

            // Get video ID from already-parsed properties
            const idProp = videoNode.numProps > 0
                ? this.readFirstPropId(videoNode)
                : 0;

            if (idProp !== 0) {
                videoDataUrls[idProp] = dataUrl;
                console.log('[FBXHelper] Texture extracted, id:', idProp,
                    'bytes:', byteLen, 'type:', mimeType);
            }
        }

        if (Object.keys(videoDataUrls).length === 0) return [];

        // Step 2 — read Connections to map material -> texture -> video
        const connectionsNode = rootNodes.find(n => n.name === 'Connections');
        const texToVideo = {};
        const matToTex = {};

        if (connectionsNode) {
            for (const conn of connectionsNode.children) {
                if (!conn || conn.numProps < 3) continue;
                try {
                    const connType = this.readPropString(conn, 0);
                    const idA = this.readPropId(conn, 1);
                    const idB = this.readPropId(conn, 2);

                    if (connType === 'OO' && videoDataUrls[idA] !== undefined)
                        texToVideo[idB] = idA;
                    if (connType === 'OP')
                        matToTex[idB] = idA;
                } catch (e) { continue; }
            }
        }

        // Step 3 — map material nodes to texture slots
        const materialNodes = objectsNode.children.filter(n => n.name === 'Material');
        const slotTextures = [];

        for (const matNode of materialNodes) {
            const matId = this.readFirstPropId(matNode);
            const texId = matToTex[matId];
            const vidId = texId !== undefined ? texToVideo[texId] : undefined;
            slotTextures.push(vidId !== undefined ? videoDataUrls[vidId] : null);
        }

        console.log('[FBXHelper] extractTextures slots:', slotTextures.length,
            'with textures:', slotTextures.filter(t => t).length);
        return slotTextures;
    },

    // Helper — read the first property as a numeric ID (Long or Int)
    readFirstPropId: function (node) {
        const offset = node.propsStart;
        const view = node.view;
        const typeCode = String.fromCharCode(view.getUint8(offset));
        if (typeCode === 'L') return Number(view.getBigInt64(offset + 1, true));
        if (typeCode === 'I') return view.getInt32(offset + 1, true);
        return 0;
    },

    // Helper — read the Nth property as a numeric ID
    readPropId: function (node, propIndex) {
        let offset = node.propsStart;
        const view = node.view;
        for (let i = 0; i < propIndex; i++) {
            const tc = String.fromCharCode(view.getUint8(offset)); offset++;
            offset += this.skipPropBytes(view, offset, tc);
        }
        const typeCode = String.fromCharCode(view.getUint8(offset)); offset++;
        if (typeCode === 'L') return Number(view.getBigInt64(offset, true));
        if (typeCode === 'I') return view.getInt32(offset, true);
        return 0;
    },

    // Helper — read the Nth property as a string
    readPropString: function (node, propIndex) {
        let offset = node.propsStart;
        const view = node.view;
        for (let i = 0; i < propIndex; i++) {
            const tc = String.fromCharCode(view.getUint8(offset)); offset++;
            offset += this.skipPropBytes(view, offset, tc);
        }
        const typeCode = String.fromCharCode(view.getUint8(offset)); offset++;
        if (typeCode === 'S') {
            const len = view.getUint32(offset, true);
            const bytes = new Uint8Array(node.buffer, offset + 4, len);
            return String.fromCharCode(...bytes);
        }
        return '';
    },

    // Helper — how many bytes does this property value occupy
    skipPropBytes: function (view, offset, typeCode) {
        switch (typeCode) {
            case 'Y': return 2;
            case 'C': return 1;
            case 'I': case 'F': return 4;
            case 'D': case 'L': return 8;
            case 'S': case 'R': return 4 + view.getUint32(offset, true);
            case 'f': case 'd': case 'l': case 'i': case 'b':
                return 12 + view.getUint32(offset + 8, true) *
                    (typeCode === 'd' || typeCode === 'l' ? 8 : 4);
            default: return 0;
        }
    },

    extractMaterialColors: function (rootNodes, objectsNode) {
        const materialNodes = objectsNode.children.filter(n => n.name === 'Material');
        console.log('[FBXHelper] extractMaterialColors — material nodes found:', materialNodes.length);
        const colors = [];

        for (const matNode of materialNodes) {
            const props70 = matNode.children.find(n => n.name === 'Properties70');
            if (!props70) { colors.push('1,1,1,1'); continue; }

            let r = 1, g = 1, b = 1;
            for (const p of props70.children) {
                const propName = this.readPropString(p, 0);
                if (propName !== 'DiffuseColor') continue;
                try {
                    r = this.readPropFloat(p, 4);
                    g = this.readPropFloat(p, 5);
                    b = this.readPropFloat(p, 6);
                } catch (e) { }
                break;
            }
            colors.push(`${r},${g},${b},1`);
        }
        console.log('[FBXHelper] extractMaterialColors result:', colors);
        return colors;
    },

    readPropFloat: function (node, propIndex) {
        let offset = node.propsStart;
        const view = node.view;
        for (let i = 0; i < propIndex; i++) {
            const tc = String.fromCharCode(view.getUint8(offset)); offset++;
            offset += this.skipPropBytes(view, offset, tc);
        }
        const typeCode = String.fromCharCode(view.getUint8(offset)); offset++;
        if (typeCode === 'D') return view.getFloat64(offset, true);
        if (typeCode === 'F') return view.getFloat32(offset, true);
        return 1;
    },








    readNode: function (buffer, view, offset, version) {
        try {
            let endOffset, numProps, propListLen;

            if (version >= 7500) {
                endOffset = Number(view.getBigUint64(offset, true)); offset += 8;
                numProps = Number(view.getBigUint64(offset, true)); offset += 8;
                propListLen = Number(view.getBigUint64(offset, true)); offset += 8;
            } else {
                endOffset = view.getUint32(offset, true); offset += 4;
                numProps = view.getUint32(offset, true); offset += 4;
                propListLen = view.getUint32(offset, true); offset += 4;
            }

            if (endOffset === 0) return null;

            const nameLen = view.getUint8(offset); offset += 1;
            const nameBytes = new Uint8Array(buffer, offset, nameLen);
            const name = String.fromCharCode(...nameBytes);
            offset += nameLen;

            // Store property offsets for lazy reading
            const propsStart = offset;
            offset += propListLen;

            // Read children
            const children = [];
            while (offset < endOffset - (version >= 7500 ? 25 : 13)) {
                const child = this.readNode(buffer, view, offset, version);
                if (!child || child.endOffset === 0) break;
                children.push(child);
                offset = child.endOffset;
            }

            return {
                name,
                endOffset,
                numProps,
                propsStart,
                propListLen,
                children,
                buffer,
                view,
                version
            };

        } catch (ex) {
            console.error('[FBXHelper] readNode error at offset:', offset, ex);
            return null;
        }
    },

    readDoubleArray: async function (node) {
        // First property of the node contains the array
        const offset = node.propsStart;
        const view = node.view;
        const typeCode = String.fromCharCode(view.getUint8(offset));

        if (typeCode === 'd') {
            return await this.readTypedArray(node.buffer, view, offset + 1, 'double');
        }
        if (typeCode === 'f') {
            return await this.readTypedArray(node.buffer, view, offset + 1, 'float');
        }
        return [];
    },

    readIntArray: async function (node) {
        const offset = node.propsStart;
        const view = node.view;
        const typeCode = String.fromCharCode(view.getUint8(offset));

        if (typeCode === 'i') {
            return await this.readTypedArray(node.buffer, view, offset + 1, 'int32');
        }
        if (typeCode === 'l') {
            return await this.readTypedArray(node.buffer, view, offset + 1, 'int64');
        }
        return [];
    },





    readTypedArray: async function (buffer, view, offset, type) {
        const arrayLength = view.getUint32(offset, true); offset += 4;
        const encoding = view.getUint32(offset, true); offset += 4;
        const compressedLength = view.getUint32(offset, true); offset += 4;

        let rawBytes;

        if (encoding === 1) {
            const compressed = buffer.slice(offset, offset + compressedLength);
            rawBytes = await this.decompress(compressed);
        } else {
            const byteSize = this.getByteSize(type);
            rawBytes = buffer.slice(offset, offset + arrayLength * byteSize);
        }

        return this.bytesToTypedArray(rawBytes, type, arrayLength);
    },

    decompress: async function (buffer) {
        const data = new Uint8Array(buffer);

        // Detect zlib header (0x78 0x9C / 0x78 0x01 / 0x78 0xDA / 0x78 0x5E)
        const hasZlibHeader = data[0] === 0x78 &&
            (data[1] === 0x9C || data[1] === 0x01 ||
                data[1] === 0xDA || data[1] === 0x5E);

        // Try zlib ('deflate' in the Streams API means deflate+zlib-header)
        if (hasZlibHeader) {
            try {
                return await this._decompress(buffer, 'deflate');
            } catch (e) {
                console.warn('[FBXHelper] zlib decompress failed, trying raw:', e);
            }
        }

        // Try raw deflate (Blender FBX typically uses this without a header)
        try {
            return await this._decompress(buffer, 'deflate-raw');
        } catch (e) {
            console.warn('[FBXHelper] raw deflate also failed:', e);
            return null;
        }
    },

    _decompress: async function (buffer, format) {
        const ds = new DecompressionStream(format);
        const writer = ds.writable.getWriter();
        const reader = ds.readable.getReader();

        writer.write(new Uint8Array(buffer));
        writer.close();

        const chunks = [];
        let totalLen = 0;
        while (true) {
            const { done, value } = await reader.read();
            if (done) break;
            chunks.push(value);
            totalLen += value.length;
        }

        const result = new Uint8Array(totalLen);
        let off = 0;
        for (const chunk of chunks) { result.set(chunk, off); off += chunk.length; }
        return result.buffer;
    },





    bytesToTypedArray: function (buffer, type, length) {
        const view = new DataView(buffer instanceof ArrayBuffer ? buffer : buffer.buffer);
        const result = [];
        let offset = 0;

        if (type === 'double') {
            for (let i = 0; i < length; i++) {
                result.push(view.getFloat64(offset, true));
                offset += 8;
            }
        } else if (type === 'float') {
            for (let i = 0; i < length; i++) {
                result.push(view.getFloat32(offset, true));
                offset += 4;
            }
        } else if (type === 'int32') {
            for (let i = 0; i < length; i++) {
                result.push(view.getInt32(offset, true));
                offset += 4;
            }
        } else if (type === 'int64') {
            for (let i = 0; i < length; i++) {
                result.push(Number(view.getBigInt64(offset, true)));
                offset += 8;
            }
        }
        return result;
    },
    processMesh: function (raw) {
        const { vertices, rawIndices, uvs, uvIndices, normals } = raw;
        if (!vertices.length || !rawIndices.length) return null;

        const outVerts = [];
        const outNorms = [];
        const outUVs = [];

        const matBreaks = [];
        const matIndices = [];
        let lastMatIdx = -1;
        let vertsAtLastBreak = 0;
        let totalVerts = 0;
        let faceCounter = 0;

        let i = 0;
        while (i < rawIndices.length) {
            // Collect polygon
            const poly = [];
            while (i < rawIndices.length) {
                const idx = rawIndices[i];
                poly.push({
                    v: idx < 0 ? ~idx : idx,
                    uvi: (uvIndices && uvIndices[i] >= 0) ? uvIndices[i] : -1,
                    ni: (raw.normalIndices && raw.normalIndices[i] >= 0)
                        ? raw.normalIndices[i]
                        : (idx < 0 ? ~idx : idx)
                });
                i++;
                if (idx < 0) break;
            }

            if (poly.length < 3) { faceCounter++; continue; }

            // Material index for this face
            const matIdx = (raw.materialIndices && raw.materialIndices[faceCounter] !== undefined)
                ? raw.materialIndices[faceCounter] : 0;

            if (matIdx !== lastMatIdx) {
                if (lastMatIdx >= 0) {
                    matBreaks.push(totalVerts - vertsAtLastBreak);
                    matIndices.push(lastMatIdx);
                    vertsAtLastBreak = totalVerts;
                }
                lastMatIdx = matIdx;
            }

            faceCounter++;

            if (poly.length === 3) {
                for (const p of [poly[0], poly[1], poly[2]]) {
                    this._pushVert(outVerts, outNorms, outUVs,
                        vertices, normals, uvs, p);
                }
            } else if (poly.length === 4) {
                const order = [poly[0], poly[3], poly[2],
                poly[0], poly[2], poly[1]];
                for (const p of order) {
                    this._pushVert(outVerts, outNorms, outUVs,
                        vertices, normals, uvs, p, true);
                }

            } else {
                // Reversed fan winding to match C# parser XYZ normal direction
                for (let t = 1; t < poly.length - 1; t++) {
                    for (const p of [poly[0], poly[t + 1], poly[t]]) {
                        this._pushVert(outVerts, outNorms, outUVs,
                            vertices, normals, uvs, p, true);
                    }
                }
            }

            totalVerts = outVerts.length / 3;
        }

        // Close final material break
        if (lastMatIdx >= 0) {
            matBreaks.push(totalVerts - vertsAtLastBreak);
            matIndices.push(lastMatIdx);
        }

        if (outVerts.length === 0) return null;

        return {
            vertices: new Float32Array(outVerts),
            normals: new Float32Array(outNorms),
            uvs: new Float32Array(outUVs),
            matBreaks,
            matIndices,
        };
    },

    // Shared vertex push — keeps processMesh readable
    _pushVert: function (outVerts, outNorms, outUVs,
        vertices, normals, uvs, p, flipNormal = false) {
        outVerts.push(
            vertices[p.v * 3],
            vertices[p.v * 3 + 1],
            vertices[p.v * 3 + 2]
        );

        if (normals.length > 0 && p.ni * 3 + 2 < normals.length) {
            const nx = normals[p.ni * 3];
            const ny = normals[p.ni * 3 + 1];
            const nz = normals[p.ni * 3 + 2];

            outNorms.push(
                flipNormal ? -nx : nx,
                flipNormal ? -ny : ny,
                flipNormal ? -nz : nz
            );

        } else {
            outNorms.push(0, 0, 1);
        }
        // FBX stores UV V-axis as bottom-origin (0=bottom, 1=top)
        // WebGL expects top-origin (0=top, 1=bottom)
        // Flip V with (1.0 - v) to correct texture display on tri faces
        // Matches the C# parser: uvLookup.Add(new Vector2(u, 1.0f - v))
        if (p.uvi >= 0 && p.uvi * 2 + 1 < uvs.length) {
            outUVs.push(uvs[p.uvi * 2], 1.0 - uvs[p.uvi * 2 + 1]);
        } else if (uvs.length > 0) {
            const fallbackIdx = (outUVs.length / 2) % (uvs.length / 2);
            outUVs.push(uvs[fallbackIdx * 2], 1.0 - uvs[fallbackIdx * 2 + 1]);
        } else {
            outUVs.push(0, 0);
        }

    },



    getByteSize: function (type) {
        if (type === 'double' || type === 'int64') return 8;
        if (type === 'float' || type === 'int32') return 4;
        return 4;
    }
};

