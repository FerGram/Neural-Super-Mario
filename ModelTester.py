import numpy as np
import onnxruntime as on

# This is a simple script to test trained ONNX models with the Unity results for debugging puposes
model_path_1 = "FinalDataChallenge.onnx"
model_path_2 = "FinalDataEngagement.onnx"
model_path_3 = "FinalDataFrustration.onnx"

model_1 = on.InferenceSession(model_path_1)
model_2 = on.InferenceSession(model_path_2)
model_3 = on.InferenceSession(model_path_3)

input_name = model_1.get_inputs()[0].name

input_np = np.array([1, 0, 1, 39, 1, 1, 1, 2, 0, 1, 2, 4, 0, 2, 1, 1, 2, 1, 2, 0, 0, 1, 0, 0, 0, 1]).astype(np.float32)
input_np = np.expand_dims(input_np, axis=0)

prediction_1 = model_1.run(None, {input_name: input_np})[0]
prediction_2 = model_2.run(None, {input_name: input_np})[0]
prediction_3 = model_3.run(None, {input_name: input_np})[0]

print(prediction_1)
print(prediction_2)
print(prediction_3)
