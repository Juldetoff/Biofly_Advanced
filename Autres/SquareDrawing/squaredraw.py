import cv2
import re

def dessiner_carre(x1, y1, x2, y2, frame):
    x1 = int(x1 * frame.shape[1])
    y1 = int((1 - y1) * frame.shape[0])
    x2 = int(x2 * frame.shape[1])
    y2 = int((1 - y2) * frame.shape[0])
    color = (0, 255, 0)
    thickness = 4
    cv2.rectangle(frame, (x1, y1), (x2, y2), color, thickness)

def dessiner_carres_sur_video(video_path, positions_path, output_path):
    # Ouvrir la vidéo
    video = cv2.VideoCapture(video_path)
    if not video.isOpened():
        print("Erreur lors de l'ouverture de la vidéo.")
        return

    # Lire les dimensions de la vidéo
    frame_width = int(video.get(cv2.CAP_PROP_FRAME_WIDTH))
    frame_height = int(video.get(cv2.CAP_PROP_FRAME_HEIGHT))
    video_fps = video.get(cv2.CAP_PROP_FPS)

    # Lire le fichier des positions
    with open(positions_path, 'r') as file:
        lines = file.readlines()

    # Créer un objet VideoWriter pour écrire la vidéo de sortie
    fourcc = cv2.VideoWriter_fourcc(*'mp4v')
    out = cv2.VideoWriter(output_path, fourcc, video_fps, (frame_width, frame_height))

    current_frame = 0
    frame_times = []
    frame_positions = []

    for line in lines:
        if line.startswith('Temps'):
            frame_time = float(line.split(':')[1].replace(',', '.').strip('s\n'))
            frame_times.append(frame_time)
            frame_positions.append([])
        else:
            match = re.findall(r'\((-?\d+\.\d+), (-?\d+\.\d+)\),\((-?\d+\.\d+), (-?\d+\.\d+)\)', line)
            for coords in match:
                x1, y1, x2, y2 = map(float, coords)
                frame_positions[-1].append((x1, y1, x2, y2))

    for frame_idx in range(len(frame_times)):
        video.set(cv2.CAP_PROP_POS_FRAMES, frame_idx)
        success, frame = video.read()
        
        for x1, y1, x2, y2 in frame_positions[frame_idx]:
            dessiner_carre(x1, y1, x2, y2, frame)
            
        out.write(frame)

    # Fermer les objets VideoCapture et VideoWriter
    video.release()
    out.release()
    #cv2.destroyAllWindows()

# Exemple d'utilisation
video_path = './camreal.mp4'
positions_path = './camrealStart.txt'
output_path = './camSquare.mp4'
dessiner_carres_sur_video(video_path, positions_path, output_path)