import sys

# https://github.com/korolkiewiczk/sentence_transformer_docker

from flask import Flask, request, jsonify
from sentence_transformers import SentenceTransformer
import models

arguments = sys.argv
model_name_arg = arguments[1]
print(f"Loading Sentence Transformer: {model_name_arg}")

app = Flask(__name__)
model = SentenceTransformer(model_name_arg)


@app.route('/v1/embeddings', methods=['POST'])
def get_embeddings():
    print("POST /v1/embeddings")
    content = request.json
    text_input = content['input']
    model_name = content.get('model', model_name_arg)

    # Encode the input text
    embedding_list = models.encode_text([text_input])[0]

    response = {
        "data": [
            {
                "embedding": embedding_list,
                "index": 0,
                "object": "embedding"
            }
        ],
        "model": model_name,
        "object": "list",
        "usage": {
            "prompt_tokens": len(text_input.split()),
            "total_tokens": len(text_input.split())
        }
    }

    return jsonify(response)

@app.route('/generate-embedding', methods=['GET'])
def generate_embedding():
    print("GET /generate-embedding")
    query = request.args.get('query')
    
    # Encode the query
    embedding_list = models.encode_text([query])[0]

    return jsonify(embedding=embedding_list)

@app.route('/generate-embeddings', methods=['POST'])
def generate_multiple_embeddings():
    print("POST /generate-embeddings")
    texts = request.json
    
    # Encode the multiple texts
    embeddings_list = models.encode_text(texts)

    return jsonify(embeddings=embeddings_list)
    
@app.route('/health', methods=['GET'])
def health():
    print("GET /health")
    return f"SBERT Model: {model_name_arg}"

@app.route('/similarity', methods=['POST'])
def similarity():
    data = request.get_json()
    sentence1 = data.get('sentence1')
    sentence2 = data.get('sentence2')
    if not sentence1 or not sentence2:
        return jsonify({'error': 'Both sentence1 and sentence2 are required.'}), 400
    similarity_score = models.calculate_similarity(sentence1, sentence2)
    return jsonify({'similarity_score': similarity_score})

if __name__ == '__main__':
    from waitress import serve
    serve(app, host="0.0.0.0", port=5000)
    #app.run(host='0.0.0.0', port=5080)