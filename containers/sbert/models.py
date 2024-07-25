import sys
from sentence_transformers import SentenceTransformer, util
model = SentenceTransformer(sys.argv[1])


def calculate_similarity(sentence1, sentence2):
    embeddings1 = model.encode(sentence1, convert_to_tensor=True)
    embeddings2 = model.encode(sentence2, convert_to_tensor=True)
    similarity_score = util.pytorch_cos_sim(embeddings1, embeddings2).item()
    return similarity_score

def encode_text(texts):
    # Prefix each text with "zapytanie: "
    prefixed_texts = ["zapytanie: " + text for text in texts]

    # Encode the texts and convert to list
    embeddings = model.encode(prefixed_texts, convert_to_tensor=True, show_progress_bar=False)
    embeddings_list = embeddings.tolist()

    return embeddings_list