
IMAGE = pullenti/pullenti-server
PORT = 8080

clean:
	rm -rf bin obj

EP.SdkCore:
	cp -r ../PullentiNetCore/EP.SdkCore .

build: EP.SdkCore
	docker build -t $(IMAGE) .

run:
	docker run -it --rm -p $(PORT):$(PORT) $(IMAGE)
